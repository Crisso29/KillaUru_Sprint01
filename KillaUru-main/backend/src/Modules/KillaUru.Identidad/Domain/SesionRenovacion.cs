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

    /// <summary>Devuelve null si la sesión sirve, o el código de error del contrato.</summary>
    internal string? Validar(DateTimeOffset ahora, bool cuentaActiva, int graciaSegundos)
    {
        ahora = HorasGuardadas.AlMicrosegundo(ahora);
        var revocada = RevocadoUtc is not null;
        var vencida = ExpiraUtc <= ahora || LimiteAbsolutoUtc <= ahora;
        // Reusar un token ya rotado fuera de la gracia indica robo (FR-016).
        var reusoFueraDeGracia = UsadoUtc is not null && ahora - UsadoUtc > TimeSpan.FromSeconds(graciaSegundos);
        if (revocada || vencida || reusoFueraDeGracia) return ErroresSesion.SesionInvalida;
        return cuentaActiva ? null : ErroresSesion.CuentaDesactivada;
    }

    internal void Usar(DateTimeOffset ahora) => UsadoUtc ??= HorasGuardadas.AlMicrosegundo(ahora);
    internal void Revocar(DateTimeOffset ahora) => RevocadoUtc ??= HorasGuardadas.ALaHora(ahora);
}
