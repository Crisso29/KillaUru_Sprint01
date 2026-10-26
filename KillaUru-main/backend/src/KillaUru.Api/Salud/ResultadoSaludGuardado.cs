using Microsoft.Extensions.Diagnostics.HealthChecks;
namespace KillaUru.Api.Salud;

/// <summary>Instantánea inmutable de salud con su vencimiento; evita estados parcialmente actualizados.</summary>
internal sealed record ResultadoSaludGuardado(HealthCheckResult Resultado, DateTimeOffset Hasta);
