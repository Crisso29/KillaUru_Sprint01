using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Domain;
using KillaUru.Shared.Eventos;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Options;
namespace KillaUru.Anomalias.Application.Procesamiento;
/// <summary>Coordina recepción y reglas; conserva idempotencia incluso si cambia el contenido de un reintento.</summary>
internal sealed class ProcesarReporteUseCase(IAnomaliaRepository anomalias, IReporteRepository reportes,
    ITransaccionAnomalias transaccion, EstadoCorredor estado, IOptions<AnomaliasOptions> opciones,
    IPublicadorEventos publicador, IMetricas metricas, TimeProvider reloj)
{
    internal async Task<ResultadoProceso> EjecutarAsync(EventoReportado evento, ContextoReporte contexto, CancellationToken ct)
    {
        var inicio = reloj.GetTimestamp();
        try
        {
            var (reporte, motivo) = MapearReporte.Desde(evento, contexto);
            var celdasABloquear = reporte is null ? [] : CeldasDelRadio(reporte);
            return await transaccion.EjecutarAsync(celdasABloquear, evento.EventoId, evento.Seudonimo,
                () => ProcesarAsync(evento.EventoId, reporte, motivo, contexto, ct), ct);
        }
        // Se mide siempre, también si el commit falla: el p95 debe incluir los casos lentos (FR-031).
        finally { metricas.RegistrarDuracionProceso(reloj.GetElapsedTime(inicio)); }
    }

    /// <summary>Celdas que toca el radio de agrupación: hay que bloquearlas todas para que dos eventos cercanos no creen dos anomalías (FR-007).</summary>
    private long[] CeldasDelRadio(ReporteAnomalia reporte) =>
        Celda.Alrededor(reporte.Posicion, opciones.Value.RadioAgrupacionMetros).Select(c => c.Id).ToArray();

    private async Task<ResultadoProceso> ProcesarAsync(Guid id, ReporteAnomalia? reporte, Domain.MotivoDescarte? motivo, ContextoReporte contexto, CancellationToken ct)
    {
        // Idempotencia por eventoId: un reintento devuelve lo mismo sin efectos (FR-006).
        if (await reportes.ExisteAsync(id, ct)) return new(YaProcesado: true);
        if (motivo is not null) return Descartar(motivo.Value);
        // Sin motivo de esquema, MapearReporte siempre devuelve el reporte.
        // Sin corredor no se descarta un evento ya recibido: se reintenta cuando la carga se recupere.
        var corredor = estado.Actual ?? throw new InvalidOperationException("Corredor aún no disponible.");
        motivo = new ReglasCalidad(opciones.Value, corredor).EvaluarIntrinseca(reporte!, contexto.RecibidoUtc);
        if (motivo is not null) return Descartar(motivo.Value);
        var anterior = await reportes.AnteriorAsync(reporte!.Seudonimo!.Value, reporte.DetectadoUtc, ct);
        motivo = new PoliticaPlausibilidad(opciones.Value).Evaluar(reporte, anterior);
        if (motivo is not null) return Descartar(motivo.Value);
        return await AgruparAsync(reporte with { KmVia = (float)corredor.KmSobreVia(reporte.Posicion) }, contexto, ct);
    }

    private async Task<ResultadoProceso> AgruparAsync(ReporteAnomalia reporte, ContextoReporte contexto, CancellationToken ct)
    {
        var reglas = opciones.Value;
        var ahora = reloj.GetUtcNow();
        var anomalia = await anomalias.MasCercanaAsync(reporte.Posicion, reporte.Tipo, reglas.RadioAgrupacionMetros, ct);
        var nueva = anomalia is null;
        // El tope solo cuenta al crear: una anomalía existente ya está a la vista de todos (FR-013).
        var porTope = nueva && new PoliticaPlausibilidad(reglas).SuperaTope(contexto.EventosEnLaHora);
        anomalia ??= Anomalia.Crear(reporte, porTope, ahora);
        var ultima = reporte.DetectadoUtc > anomalia.UltimaDeteccionUtc ? reporte.DetectadoUtc : anomalia.UltimaDeteccionUtc;
        var historial = nueva ? [] : await reportes.DesdeAsync(anomalia.Id, ultima.AddDays(-DiasDeHistorialNecesarios(reglas)), ct);
        anomalia.RegistrarReporte(reporte, [.. historial, reporte], reglas, ahora);
        reportes.Agregar(reporte, anomalia.Id);
        await anomalias.GuardarAsync(anomalia, nueva, ct);
        transaccion.AlConfirmar(() => publicador.PublicarAsync(new AnomaliaCambiada(anomalia.Id, anomalia.Celda), CancellationToken.None));
        if (porTope) transaccion.AlConfirmar(() => { metricas.RegistrarRetenidoPorTope(); return Task.CompletedTask; });
        return new();
    }

    /// <summary>
    /// Se carga la ventana más larga que usa alguna regla (persistencia 15 d, severidad 15 d,
    /// revisión 7 d, confirmación 72 h) para que ninguna decida con datos incompletos.
    /// </summary>
    private static double DiasDeHistorialNecesarios(AnomaliasOptions reglas) => new[]
    {
        reglas.VentanaDias, reglas.VentanaSeveridadDias, reglas.VentanaRevisionDescartadaDias, reglas.VentanaConfirmacionHoras / 24.0,
    }.Max();

    /// <summary>Los descartes se cuentan como métrica por motivo y no se auditan (FR-005).</summary>
    private ResultadoProceso Descartar(Domain.MotivoDescarte motivo)
    {
        transaccion.AlConfirmar(() => { metricas.RegistrarDescarte(motivo.ToString()); return Task.CompletedTask; });
        return new(Motivo: (Contracts.MotivoDescarte)motivo);
    }
}
