using KillaUru.Api.Configuracion;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace KillaUru.Api.Observabilidad;

/// <summary>
/// Logs estructurados en JSON (RNF-21), nivel Information o superior. El log de finalización de
/// /health se emite como Verbose y queda filtrado. Envía por OTLP si hay endpoint configurado.
/// </summary>
/// <remarks>
/// FromLogContext incorpora propiedades aportadas por otros componentes; aquí no se agrega
/// HttpContext.TraceIdentifier como CorrelacionId a cada petición. El manejador de errores sí lo registra.
/// </remarks>
internal static class LogsExtensions
{
    internal static WebApplicationBuilder AgregarLogs(this WebApplicationBuilder builder)
    {
        var telemetria = builder.Configuration.GetSection(ApiOptions.Seccion).Get<ApiOptions>()?.Telemetria ?? new();
        builder.Services.AddSerilog(configuracion =>
        {
            configuracion
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.With<FiltroDatosPersonales>()
                .WriteTo.Console(new RenderedCompactJsonFormatter());
            if (telemetria.OtlpEndpoint is { } endpoint)
            {
                configuracion.WriteTo.OpenTelemetry(opciones => opciones.Endpoint = endpoint.ToString());
            }
        });
        return builder;
    }

    internal static WebApplication UsarLogsDePeticiones(this WebApplication app)
    {
        app.UseSerilogRequestLogging(opciones => opciones.GetLevel = (contexto, _, error) =>
            contexto.Request.Path.StartsWithSegments("/health") ? LogEventLevel.Verbose
            : error is not null || contexto.Response.StatusCode >= 500 ? LogEventLevel.Error
            : LogEventLevel.Information);
        return app;
    }
}
