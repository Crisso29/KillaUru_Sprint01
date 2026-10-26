namespace KillaUru.Anomalias.Contracts;
/// <summary>Datos de recepción; ConSesion nunca identifica una cuenta y el conteo usa la hora de detección.</summary>
public sealed record ContextoReporte(bool ConSesion, int EventosEnLaHora, DateTimeOffset RecibidoUtc);
