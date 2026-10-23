namespace KillaUru.Api.Configuracion;

/// <summary>Configuración del host (sección <c>Api</c>, data-model §4). Cada módulo tiene la suya.</summary>
internal sealed class ApiOptions
{
    internal const string Seccion = "Api";

    /// <summary>Cuotas por rol y techos por IP (RNF-11, D2, D8).</summary>
    public OpcionesLimiteTasa LimiteTasa { get; init; } = new();

    /// <summary>Envío de telemetría a Grafana Cloud (RNF-21, RNF-32).</summary>
    public OpcionesTelemetria Telemetria { get; init; } = new();

    /// <summary>Validación de los tokens de acceso (ADR-009).</summary>
    public OpcionesJwt Jwt { get; init; } = new();

    /// <summary>Habilita /openapi y /docs; debe permanecer desactivada en producción (FR-015).</summary>
    public bool DocumentacionPublica { get; init; }

    /// <summary>Único origen al que CORS permite credenciales (RNF-09).</summary>
    public string OrigenPanel { get; init; } = "https://panel.killauru.pe";

    /// <summary>Red (CIDR) de la que se acepta CF-Connecting-IP: la de cloudflared (RNF-11).</summary>
    public string? RedTunel { get; init; }
}
