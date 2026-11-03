using KillaUru.Anomalias.Contracts;
using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Ingesta.Domain;
using KillaUru.Shared.Metricas;

namespace KillaUru.Ingesta.Application.Procesamiento;

/// <summary>
/// Carga las filas Recibido de un lote, las entrega a Anomalías en orden de detectado_utc con
/// el contexto correcto, y marca cada una al recibir la respuesta (FR-012…FR-014, FR-026).
/// Un fallo en una entrega deja la fila Recibido y sigue con la siguiente (R-015).
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
        foreach (var pendiente in ordenados)
            await IntentarProcesarAsync(pendiente, ct);
    }

    private async Task IntentarProcesarAsync(EventoPendiente pendiente, CancellationToken ct)
    {
        try
        {
            var contexto = await armador.ArmarAsync(pendiente, ct);
            var resultado = await anomalias.ProcesarReporteAsync(pendiente.Evento, contexto, ct);
            var estado = resultado.Motivo.HasValue ? EstadoRecepcion.Descartado : EstadoRecepcion.Procesado;
            var motivo = resultado.Motivo.HasValue ? (short?)resultado.Motivo.Value : null;
            await repositorio.MarcarAsync(pendiente.Evento.EventoId, estado, motivo, ct);
            var retraso = reloj.GetUtcNow() - pendiente.RecibidoUtc;
            metricas.RegistrarRetrasoCola(retraso);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch { /* la fila sigue Recibido; el barredor la reintenta */ }
    }
}
