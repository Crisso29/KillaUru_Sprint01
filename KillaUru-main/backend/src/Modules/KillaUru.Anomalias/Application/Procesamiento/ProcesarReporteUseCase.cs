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
            var celdas = reporte is null ? [] : Celda.Alrededor(reporte.Posicion, opciones.Value.RadioAgrupacionMetros).Select(c => c.Id).ToArray();
            return await transaccion.EjecutarAsync(celdas, evento.EventoId, evento.Seudonimo,
                () => ProcesarAsync(evento.EventoId, reporte, motivo, contexto, ct), ct);
        }
        finally { metricas.RegistrarDuracionProceso(reloj.GetElapsedTime(inicio)); }
    }

    private async Task<ResultadoProceso> ProcesarAsync(Guid id, ReporteAnomalia? reporte, Domain.MotivoDescarte? motivo, ContextoReporte contexto, CancellationToken ct)
    {
        if (await reportes.ExisteAsync(id, ct)) return new(YaProcesado: true);
        if (motivo is not null) return Descartar(motivo.Value);
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
        var o = opciones.Value;
        var anomalia = await anomalias.MasCercanaAsync(reporte.Posicion, reporte.Tipo, o.RadioAgrupacionMetros, ct);
        var nueva = anomalia is null;
        var porTope = nueva && new PoliticaPlausibilidad(o).SuperaTope(contexto.EventosEnLaHora);
        anomalia ??= Anomalia.Crear(reporte, porTope, reloj.GetUtcNow());
        var ultima = reporte.DetectadoUtc > anomalia.UltimaDeteccionUtc ? reporte.DetectadoUtc : anomalia.UltimaDeteccionUtc;
        var dias = Math.Max(Math.Max(o.VentanaDias, o.VentanaSeveridadDias), Math.Max(o.VentanaRevisionDescartadaDias, o.VentanaConfirmacionHoras / 24.0));
        var ventana = nueva ? [] : await reportes.DesdeAsync(anomalia.Id, ultima.AddDays(-dias), ct);
        anomalia.RegistrarReporte(reporte, [.. ventana, reporte], o, reloj.GetUtcNow());
        reportes.Agregar(reporte, anomalia.Id);
        await anomalias.GuardarAsync(anomalia, nueva, ct);
        transaccion.AlConfirmar(() => publicador.PublicarAsync(new AnomaliaCambiada(anomalia.Id, anomalia.Celda), CancellationToken.None));
        if (porTope) transaccion.AlConfirmar(() => { metricas.RegistrarRetenidoPorTope(); return Task.CompletedTask; });
        return new();
    }

    private ResultadoProceso Descartar(Domain.MotivoDescarte motivo)
    {
        transaccion.AlConfirmar(() => { metricas.RegistrarDescarte(motivo.ToString()); return Task.CompletedTask; });
        return new(Motivo: (Contracts.MotivoDescarte)motivo);
    }
}
