namespace KillaUru.Anomalias.Contracts.Lecturas;
/// <summary>Filtros combinables; fechas sobre última detección y estados activos por defecto.</summary>
public sealed record FiltroAnomalias(double MinLon, double MinLat, double MaxLon, double MaxLat,
    TipoAnomalia? Tipo = null, int[]? Severidades = null, DateTimeOffset? DesdeUtc = null,
    DateTimeOffset? HastaUtc = null, EstadoAnomalia[]? Estados = null);
