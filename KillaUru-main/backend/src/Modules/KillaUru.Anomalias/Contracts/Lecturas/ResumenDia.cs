namespace KillaUru.Anomalias.Contracts.Lecturas;
/// <summary>Conteos agregados del día de Lima; eventos recibidos significa reportes válidos procesados.</summary>
public sealed record ResumenDia(DateOnly Dia, long EventosRecibidos, long DispositivosDistintos,
    int TramosConReportes7d, int TramosTotales, long Confirmaciones, long PendientesExpiradas,
    long Expiraciones, long Renacimientos);
