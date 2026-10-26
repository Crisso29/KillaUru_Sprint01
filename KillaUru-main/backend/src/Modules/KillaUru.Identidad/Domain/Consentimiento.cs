namespace KillaUru.Identidad.Domain;

/// <summary>Aceptación inmutable de una versión, con precisión de día local (D3).</summary>
internal sealed class Consentimiento
{
    public Guid UsuarioId { get; private set; }
    public DocumentoLegal Documento { get; private set; }
    public string Version { get; private set; } = "";
    public DateTimeOffset AceptadoUtc { get; private set; }
    internal static Consentimiento Crear(Guid usuario, DocumentoLegal documento, string version, DateTimeOffset ahora) =>
        new() { UsuarioId = usuario, Documento = documento, Version = version, AceptadoUtc = HorasGuardadas.AlDia(ahora) };
}
