namespace KillaUru.Identidad.Domain;

/// <summary>Una generación de la familia rotativa; nunca contiene el secreto en claro.</summary>
internal sealed class SesionRenovacion
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid FamiliaId { get; private set; }
    public byte[] HashToken { get; private set; } = [];
    public DateTimeOffset CreadoUtc { get; private set; }
    public DateTimeOffset ExpiraUtc { get; private set; }
    public DateTimeOffset? LimiteAbsolutoUtc { get; private set; }
    public DateTimeOffset? UsadoUtc { get; private set; }
    public DateTimeOffset? RevocadoUtc { get; private set; }
    internal static SesionRenovacion Crear(Guid usuario, Guid familia, byte[] hash, DateTimeOffset ahora,
        DateTimeOffset expira, DateTimeOffset? limite) => new()
        {
            Id = Guid.CreateVersion7(ahora), UsuarioId = usuario, FamiliaId = familia, HashToken = hash.ToArray(),
            CreadoUtc = HorasGuardadas.ALaHora(ahora), ExpiraUtc = HorasGuardadas.ALaHora(expira), LimiteAbsolutoUtc = limite,
        };

    internal string? Validar(DateTimeOffset ahora, bool activo, int graciaSegundos)
    {
        ahora = HorasGuardadas.AlMicrosegundo(ahora);
        if (RevocadoUtc is not null || ExpiraUtc <= ahora || LimiteAbsolutoUtc <= ahora) return "sesion_invalida";
        if (UsadoUtc is not null && ahora - UsadoUtc > TimeSpan.FromSeconds(graciaSegundos)) return "sesion_invalida";
        return activo ? null : "cuenta_desactivada";
    }

    internal void Usar(DateTimeOffset ahora) => UsadoUtc ??= HorasGuardadas.AlMicrosegundo(ahora);
    internal void Revocar(DateTimeOffset ahora) => RevocadoUtc ??= HorasGuardadas.ALaHora(ahora);
}
