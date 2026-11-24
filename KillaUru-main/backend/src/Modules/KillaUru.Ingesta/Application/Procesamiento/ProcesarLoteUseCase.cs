using KillaUru.Anomalias.Contracts;
using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Ingesta.Domain;
using KillaUru.Shared.Metricas;

namespace KillaUru.Ingesta.Application.Procesamiento;

/// <summary>
/// Carga las filas Recibido de un lote, las entrega a Anomalías en orden de detectado_utc con
/// el contexto correcto, y marca todas las exitosas en un único commit al final (FR-012…FR-014,
/// FR-026). Un fallo en una entrega deja la fila Recibido y sigue con la siguiente (R-015).
/// Un solo commit WAL por lote en vez de uno por evento (DT-01, Fix A).
/// </summary>
internal sealed class ProcesarLoteUseCase(
    IPendientesRepositorio repositorio,
    IAnomaliasService anomalias,
    ArmadorContexto armador,
    IMetricas metricas,
    TimeProvider reloj)
{
    internal async Task EjecutarAsync(Guid? loteId, CancellationToken ct)
    {
        var pendientes = await repositorio.RecibidosDelLoteAsync(loteId, ct);
        var ordenados = pendientes.OrderBy(p => p.Evento.DetectadoUtc).ThenBy(p => p.Evento.EventoId);
        var marcas = new List<MarcaEvento>();
        foreach (var pendiente in ordenados)
        {
            var marca = await IntentarProcesarAsync(pendiente, ct);
            if (marca is not null) marcas.Add(marca);
        }
        if (marcas.Count > 0) await repositorio.MarcarLoteAsync(marcas, ct);
    }

    // Devuelve la marca si el evento se procesó (con o sin motivo de descarte), null si lanzó excepción.
    private async Task<MarcaEvento?> IntentarProcesarAsync(EventoPendiente pendiente, CancellationToken ct)
    {
        try
        {
            var contexto = await armador.ArmarAsync(pendiente, ct);
            var resultado = await anomalias.ProcesarReporteAsync(pendiente.Evento, contexto, ct);
            var estado = resultado.Motivo.HasValue ? EstadoRecepcion.Descartado : EstadoRecepcion.Procesado;
            var motivo = resultado.Motivo.HasValue ? (short?)resultado.Motivo.Value : null;
            var retraso = reloj.GetUtcNow() - pendiente.RecibidoUtc;
            metricas.RegistrarRetrasoCola(retraso);
            return new MarcaEvento(pendiente.Evento.EventoId, estado, motivo);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch { return null; /* la fila sigue Recibido; el barredor la reintenta */ }
    }
}
