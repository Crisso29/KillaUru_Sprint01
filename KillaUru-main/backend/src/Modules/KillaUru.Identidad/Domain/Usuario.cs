namespace KillaUru.Identidad.Domain;

/// <summary>Cuenta sin vínculo con eventos; delega RN-19 a su política de bloqueo.</summary>
internal sealed class Usuario
{
    public Guid Id { get; private set; }
    public Correo Correo { get; private set; } = new("");
    public string? HashContrasena { get; private set; }
    public RolUsuario Rol { get; private set; }
    public bool Activo { get; private set; } = true;
    public bool CorreoVerificado { get; private set; }
    public DateTimeOffset CreadoUtc { get; private set; }
    public PoliticaBloqueo Bloqueo { get; private set; } = new();
    internal string Estado => (HashContrasena, Activo) switch
    {
        (null, _) => "InvitacionPendiente",
        (_, true) => "Activa",
        _ => "Desactivada",
    };

    internal static Usuario RegistrarConductor(Correo correo, string hash, DateTimeOffset ahora)
    {
        var usuario = Invitar(correo, RolUsuario.ConductorRegistrado, ahora);
        usuario.FijarContrasena(hash);
        return usuario;
    }

    internal static Usuario Invitar(Correo correo, RolUsuario rol, DateTimeOffset ahora) => new()
    {
        Id = Guid.CreateVersion7(ahora), Correo = correo, Rol = rol, CreadoUtc = HorasGuardadas.AlDia(ahora),
    };

    internal void FijarContrasena(string hash)
    {
        HashContrasena = hash;
        Bloqueo.RegistrarExito();
    }

    internal void MarcarCorreoVerificado() => CorreoVerificado = true;
    internal void CambiarRol(RolUsuario rol) => Rol = rol;
    internal void Desactivar() => Activo = false;
    internal void Activar() => Activo = true;
    internal bool PuedeIniciarSesion(DateTimeOffset ahora)
    {
        var tieneContrasena = HashContrasena is not null;
        return Activo && tieneContrasena && !Bloqueo.EstaBloqueado(ahora);
    }
}
