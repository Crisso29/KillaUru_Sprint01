namespace KillaUru.Anomalias.Contracts;
/// <summary>Resultado estable para que Ingesta confirme su propio estado después del commit de Anomalías.</summary>
public sealed record ResultadoProceso(bool YaProcesado = false, MotivoDescarte? Motivo = null);
