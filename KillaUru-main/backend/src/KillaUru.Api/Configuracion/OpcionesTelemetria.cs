namespace KillaUru.Api.Configuracion;

/// <summary>Telemetría dentro del plan gratuito de Grafana Cloud (RNF-32).</summary>
internal sealed class OpcionesTelemetria
{
    /// <summary>Fracción de trazas que se envían (≤ 10 %).</summary>
    public double MuestreoTrazas { get; init; } = 0.10;

    /// <summary>Endpoint OTLP de Grafana Cloud. Vacío = no se envía nada (desarrollo y pruebas).</summary>
    public Uri? OtlpEndpoint { get; init; }
}
