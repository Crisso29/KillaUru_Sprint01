namespace KillaUru.Anomalias.Domain;
/// <summary>Entrada cruda: un campo inválido rechaza solo este evento, no todo el lote.</summary>
internal sealed record ReporteEntrada(Guid EventoId, DateTimeOffset? DetectadoUtc, double? Latitud,
    double? Longitud, double? PrecisionMetros, string? Tipo, int? Severidad, double? Confidence,
    string? ModeloVersion, Guid? Seudonimo);
