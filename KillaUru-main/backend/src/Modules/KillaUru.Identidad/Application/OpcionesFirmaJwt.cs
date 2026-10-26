namespace KillaUru.Identidad.Application;

/// <summary>Material de firma compartido con el host por configuración, sin dependencia hacia la API.</summary>
internal sealed class OpcionesFirmaJwt
{
    public string Emisor { get; init; } = "https://api.killauru.pe";
    public string Audiencia { get; init; } = "killauru";
    public string ClaveFirma { get; init; } = "";
}
