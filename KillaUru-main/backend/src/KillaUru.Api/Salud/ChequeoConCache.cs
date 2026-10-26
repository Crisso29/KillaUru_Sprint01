using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KillaUru.Api.Salud;

/// <summary>
/// Plantilla (Template Method) de un chequeo que consulta la base: guarda el último resultado 5 s
/// para que UptimeRobot o un balanceador no carguen la BD con cada consulta a /health/ready.
/// Las subclases solo dicen QUÉ consultar. Sin candado a propósito: si dos peticiones llegan a la vez,
/// ambas consultan una vez, lo que es inofensivo y evita un recurso descartable.
/// </summary>
/// <param name="reloj">Reloj del sistema (TimeProvider, constitución III).</param>
internal abstract class ChequeoConCache(TimeProvider reloj) : IHealthCheck
{
    private static readonly TimeSpan Vigencia = TimeSpan.FromSeconds(5);
    private ResultadoSaludGuardado? _ultimo;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var ahora = reloj.GetUtcNow();
        if (_ultimo is { } guardado && guardado.Hasta > ahora)
        {
            return guardado.Resultado;
        }

        var resultado = await ConsultarSeguroAsync(cancellationToken).ConfigureAwait(false);
        _ultimo = new ResultadoSaludGuardado(resultado, ahora + Vigencia);
        return resultado;
    }

    /// <summary>La consulta propia del chequeo.</summary>
    /// <param name="ct">Cancelación.</param>
    /// <returns>Sano o no sano; nunca con datos internos en la descripción.</returns>
    protected abstract Task<HealthCheckResult> ConsultarAsync(CancellationToken ct);

    private async Task<HealthCheckResult> ConsultarSeguroAsync(CancellationToken ct)
    {
        try
        {
            return await ConsultarAsync(ct).ConfigureAwait(false);
        }
#pragma warning disable CA1031 // Cualquier falla de la BD significa «no listo»: se informa, no se propaga.
        catch (Exception ex) when (ex is not OperationCanceledException)
#pragma warning restore CA1031
        {
            return HealthCheckResult.Unhealthy("No disponible", ex);
        }
    }

}
