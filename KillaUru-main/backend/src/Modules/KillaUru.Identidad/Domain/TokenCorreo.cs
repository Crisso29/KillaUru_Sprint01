namespace KillaUru.Identidad.Domain;

/// <summary>Enlace de un solo uso; la vigencia se calcula con el reloj recibido.</summary>
internal sealed class TokenCorreo
{
    public byte[] HashToken { get; private set; } = [];
    public Guid UsuarioId { get; private set; }
    public PropositoToken Proposito { get; private set; }
    public DateTimeOffset ExpiraUtc { get; private set; }
    public DateTimeOffset? UsadoUtc { get; private set; }
    internal static TokenCorreo Crear(byte[] hash, Guid usuario, PropositoToken proposito,
        DateTimeOffset ahora, TimeSpan vigencia) => new()
        {
            HashToken = hash.ToArray(), UsuarioId = usuario, Proposito = proposito, ExpiraUtc = ahora.Add(vigencia),
        };
    internal bool PuedeUsarse(DateTimeOffset ahora) => UsadoUtc is null && ahora < ExpiraUtc;
    internal void Usar(DateTimeOffset ahora) => UsadoUtc ??= ahora;
}
