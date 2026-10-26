namespace KillaUru.Anomalias.Contracts;
/// <summary>Entrada cruda: un campo inválido rechaza solo este evento, no todo el lote.</summary>
public sealed record EventoReportado(Guid EventoId, DateTimeOffset? DetectadoUtc, double? Latitud,
    double? Longitud, double? PrecisionMetros, string? Tipo, int? Severidad, double? Confidence,
    string? ModeloVersion, Guid? Seudonimo);
