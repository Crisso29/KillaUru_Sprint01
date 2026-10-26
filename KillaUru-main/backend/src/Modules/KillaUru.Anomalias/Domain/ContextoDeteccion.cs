namespace KillaUru.Anomalias.Domain;
/// <summary>Datos de recepción; ConSesion nunca identifica una cuenta y el conteo usa la hora de detección.</summary>
internal sealed record ContextoDeteccion(bool ConSesion, int EventosEnLaHora, DateTimeOffset RecibidoUtc);
