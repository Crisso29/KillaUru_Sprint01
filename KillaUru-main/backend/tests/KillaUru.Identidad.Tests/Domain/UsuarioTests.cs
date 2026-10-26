using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Tests.Domain;

/// <summary>
/// Ciclo de vida de la cuenta: invitación sin contraseña, activa, desactivada y reactivada.
/// Solo una cuenta activa, con contraseña y sin bloqueo puede iniciar sesión.
/// Spec 003 · FR-001, FR-031, FR-032, FR-039.
/// </summary>
public sealed class UsuarioTests
{
    private static readonly Correo CorreoValido = Correo.Crear("conductor@example.com")!;

    /// <summary>El registro público siempre crea un conductor: el rol nunca viene del cliente (FR-001).</summary>
    [Fact]
    public void Registrar_crea_conductor_activo_con_contrasena()
    {
        // Act
        var usuario = Usuario.RegistrarConductor(CorreoValido, "hash", Instantes.Ahora);

        // Assert
        Assert.NotEqual(Guid.Empty, usuario.Id);
        Assert.Equal(RolUsuario.ConductorRegistrado, usuario.Rol);
        Assert.Equal("hash", usuario.HashContrasena);
        Assert.Equal("Activa", usuario.Estado);
        Assert.True(usuario.PuedeIniciarSesion(Instantes.Ahora));
    }

    /// <summary>La fecha de alta se guarda truncada al día local para no correlacionarla con eventos (FR-039).</summary>
    [Fact]
    public void Alta_se_guarda_truncada_al_dia()
    {
        // Act
        var usuario = Usuario.Invitar(CorreoValido, RolUsuario.Administrador, Instantes.Ahora);

        // Assert
        Assert.Equal(CorreoValido, usuario.Correo);
        Assert.Equal(HorasGuardadas.AlDia(Instantes.Ahora), usuario.CreadoUtc);
    }

    /// <summary>Una invitación sin contraseña fijada no permite entrar (FR-031).</summary>
    [Fact]
    public void Invitacion_pendiente_no_puede_iniciar_sesion()
    {
        // Act
        var usuario = Usuario.Invitar(CorreoValido, RolUsuario.Administrador, Instantes.Ahora);

        // Assert
        Assert.Equal("InvitacionPendiente", usuario.Estado);
        Assert.False(usuario.PuedeIniciarSesion(Instantes.Ahora));
    }

    /// <summary>Fijar la contraseña de la invitación activa la cuenta (FR-031).</summary>
    [Fact]
    public void Fijar_contrasena_y_verificar_activa_la_invitacion()
    {
        // Arrange
        var usuario = Usuario.Invitar(CorreoValido, RolUsuario.Administrador, Instantes.Ahora);

        // Act
        usuario.FijarContrasena("hash");
        usuario.MarcarCorreoVerificado();

        // Assert
        Assert.True(usuario.CorreoVerificado);
        Assert.Equal("Activa", usuario.Estado);
        Assert.True(usuario.PuedeIniciarSesion(Instantes.Ahora));
    }

    /// <summary>Una cuenta desactivada por el administrador no puede entrar (FR-032).</summary>
    [Fact]
    public void Desactivar_impide_iniciar_sesion()
    {
        // Arrange
        var usuario = Usuario.RegistrarConductor(CorreoValido, "hash", Instantes.Ahora);

        // Act
        usuario.Desactivar();

        // Assert
        Assert.False(usuario.Activo);
        Assert.Equal("Desactivada", usuario.Estado);
        Assert.False(usuario.PuedeIniciarSesion(Instantes.Ahora));
    }

    /// <summary>El administrador puede reactivar y cambiar el rol de una cuenta (FR-032).</summary>
    [Fact]
    public void Activar_y_cambiar_rol_restauran_la_cuenta()
    {
        // Arrange
        var usuario = Usuario.Invitar(CorreoValido, RolUsuario.Administrador, Instantes.Ahora);
        usuario.Desactivar();

        // Act
        usuario.Activar();
        usuario.CambiarRol(RolUsuario.ConductorRegistrado);

        // Assert
        Assert.True(usuario.Activo);
        Assert.Equal(RolUsuario.ConductorRegistrado, usuario.Rol);
    }
}
