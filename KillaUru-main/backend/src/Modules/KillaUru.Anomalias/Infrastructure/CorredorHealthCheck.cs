using KillaUru.Anomalias.Application.Procesamiento;
using Microsoft.Extensions.Diagnostics.HealthChecks;
namespace KillaUru.Anomalias.Infrastructure;
/// <summary>No consulta la base en cada petición; solo comprueba la instantánea cargada.</summary>
internal sealed class CorredorHealthCheck(EstadoCorredor estado) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult(estado.Actual is { LongitudKm: > 0 } ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());
}
