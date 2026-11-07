using KillaUru.Ingesta.Application.Puertos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace KillaUru.Ingesta.Infrastructure;

/// <summary>
/// Falla si existe una fila Recibido con más de AntiguedadMaximaColaMinutos (RF-28, FR-021).
/// Usa caché de 5 s para no consultar la BD en cada petición de salud.
/// </summary>
internal sealed class ChequeoColaIngesta(
    IServiceScopeFactory scopes,
    TimeProvider reloj,
    IOptions<Application.IngestaOptions> opciones) : IHealthCheck
{
    private HealthCheckResult? _cache;
    private DateTimeOffset _ultimaConsulta = DateTimeOffset.MinValue;
    private static readonly TimeSpan TtlCache = TimeSpan.FromSeconds(5);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct)
    {
        var ahora = reloj.GetUtcNow();
        if (_cache.HasValue && ahora - _ultimaConsulta < TtlCache) return _cache.Value;

        // El chequeo conserva su caché, pero el DbContext pertenece solo a esta consulta.
        await using var scope = scopes.CreateAsyncScope();
        var barrido = scope.ServiceProvider.GetRequiredService<IBarridoPendientes>();
        var masAntiguo = await barrido.MasAntiguoPendienteAsync(ct);
        var umbral = TimeSpan.FromMinutes(opciones.Value.AntiguedadMaximaColaMinutos);

        _cache = masAntiguo.HasValue && ahora - masAntiguo.Value > umbral
            ? HealthCheckResult.Unhealthy($"Pendiente más viejo: {masAntiguo.Value:O}")
            : HealthCheckResult.Healthy();
        _ultimaConsulta = ahora;
        return _cache.Value;
    }
}
