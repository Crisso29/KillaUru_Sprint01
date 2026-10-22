namespace KillaUru.Api.Configuracion;

/// <summary>Validación del token de acceso de 5 min (ADR-009). La emisión es de Identidad (003).</summary>
internal sealed class OpcionesJwt
{
    /// <summary>Emisor esperado.</summary>
    public string Emisor { get; init; } = "https://api.killauru.pe";

    /// <summary>Audiencia esperada.</summary>
    public string Audiencia { get; init; } = "killauru";

    /// <summary>Clave HMAC de firma (secreto KU_JWT_CLAVE_FIRMA, RNF-34). Mínimo 32 bytes.</summary>
    public string ClaveFirma { get; init; } = string.Empty;
}
