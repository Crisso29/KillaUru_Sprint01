using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KillaUru.Api.Salud;

/// <summary>
/// /health/live (Docker: ¿el proceso responde?, sin tocar la BD) y /health/ready (UptimeRobot y el
/// despliegue: BD, migraciones y los chequeos que agrega cada módulo con la etiqueta <c>ready</c>).
/// Ninguna respuesta lleva datos internos: solo el estado (RF-28, FR-018, FR-019).
/// </summary>
internal static class SaludExtensions
{
    internal const string EtiquetaReady = "ready";

    internal static IServiceCollection AgregarSalud(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<ChequeoBaseDatos>("base-datos", tags: [EtiquetaReady])
            .AddCheck<ChequeoMigraciones>("migraciones", tags: [EtiquetaReady]);
        services.AddSingleton<ChequeoBaseDatos>();
        services.AddSingleton<ChequeoMigraciones>();
        return services;
    }

    internal static WebApplication MapearSalud(this WebApplication app)
    {
        var salud = app.MapGroup("/health").WithTags("Salud").AllowAnonymous().DisableRateLimiting();
        salud.MapGet("/live", () => Results.Text("Healthy"))
            .WithName("saludViva")
            .WithSummary("El proceso responde")
            .WithDescription("Rol: público. Lo usa Docker. No consulta la base de datos.")
            .Produces<string>(StatusCodes.Status200OK, "text/plain");
        salud.MapGet("/ready", ListoAsync)
            .WithName("saludLista")
            .WithSummary("La API puede atender")
            .WithDescription("Rol: público. Lo usan UptimeRobot y el despliegue: base de datos, migraciones y módulos.")
            .Produces<string>(StatusCodes.Status200OK, "text/plain")
            .Produces<string>(StatusCodes.Status503ServiceUnavailable, "text/plain");
        return app;
    }

    private static async Task<IResult> ListoAsync(HealthCheckService salud, CancellationToken ct)
    {
        var informe = await salud.CheckHealthAsync(c => c.Tags.Contains(EtiquetaReady), ct).ConfigureAwait(false);
        return informe.Status == HealthStatus.Healthy
            ? Results.Text("Healthy")
            : Results.Text("Unhealthy", statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}
