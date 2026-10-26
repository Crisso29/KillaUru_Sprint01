namespace KillaUru.Anomalias.Contracts;
/// <summary>Frontera de Ingesta hacia las reglas y procesamiento de Anomalías.</summary>
public interface IAnomaliasService
{
    /// <summary>Valida sin base de datos usando la hora original de recepción; null indica aceptación.</summary>
    MotivoDescarte? EvaluarIntrinseca(EventoReportado evento, DateTimeOffset recibidoUtc);
    /// <summary>Procesa idempotentemente con transacción y bloqueo propios; publica cambios tras confirmar.</summary>
    Task<ResultadoProceso> ProcesarReporteAsync(EventoReportado evento, ContextoReporte contexto, CancellationToken ct);
}
