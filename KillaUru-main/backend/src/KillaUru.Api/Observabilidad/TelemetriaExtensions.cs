using KillaUru.Api.Configuracion;
using KillaUru.Shared.Metricas;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace KillaUru.Api.Observabilidad;

/// <summary>
/// OpenTelemetry (RNF-21, ADR-010): métricas de ASP.NET Core (con <c>http.route</c>, nunca la URL con
/// ids) y las de negocio de <see cref="IMetricas"/>; trazas con muestreo ≤ 10 %. Se EMPUJAN a Grafana
/// Cloud por OTLP: no se expone /metrics ni se abre ningún puerto (FR-021).
/// </summary>
internal static class TelemetriaExtensions
{
    internal static IServiceCollection AgregarTelemetria(this IServiceCollection services, IConfiguration configuracion)
    {
        var telemetria = configuracion.GetSection(ApiOptions.Seccion).Get<ApiOptions>()?.Telemetria ?? new();
        services.AddSingleton<IMetricas, MetricasOpenTelemetry>();
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("killauru-api"))
            .WithMetrics(metricas =>
            {
                metricas.AddAspNetCoreInstrumentation().AddMeter(MetricasOpenTelemetry.NombreMedidor);
                if (telemetria.OtlpEndpoint is { } endpoint)
                {
                    metricas.AddOtlpExporter(o => o.Endpoint = endpoint);
                }
            })
            .WithTracing(trazas =>
            {
                trazas
                    .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(telemetria.MuestreoTrazas)))
                    .AddAspNetCoreInstrumentation(o => o.Filter = c => !c.Request.Path.StartsWithSegments("/health"));
                if (telemetria.OtlpEndpoint is { } endpoint)
                {
                    trazas.AddOtlpExporter(o => o.Endpoint = endpoint);
                }
            });
        return services;
    }
}
