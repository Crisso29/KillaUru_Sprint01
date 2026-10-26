using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application.Usuarios;
using KillaUru.Identidad.Domain;
using NSubstitute;

namespace KillaUru.Identidad.Tests.Application;

/// <summary>
/// El administrador cambia rol, desactiva y activa cuentas; quitar privilegios revoca todas las
/// sesiones al momento y cada acción queda auditada. Spec 003 · FR-019, FR-032, FR-038.
/// </summary>
public sealed class GestionUsuariosTests
{
    private readonly EscenarioAplicacion _escenario = new();

    private Task AuditadoUnaVez(Usuario usuario) => _escenario.Auditoria.Received(1).RegistrarAsync(Arg.Any<AccionAuditada>(),
        Arg.Any<ActorAuditoria>(), usuario.Id.ToString(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>());

    /// <summary>Un cambio de rol corta las sesiones para que el rol viejo no siga vigente en el JWT (FR-019).</summary>
    [Fact]
    public async Task Cambiar_rol_revoca_sesiones_y_audita()
    {
        // Arrange
        var usuario = _escenario.Conductor();
        _escenario.Existen(usuario);
        var caso = new CambiarRolUseCase(_escenario.Usuarios, _escenario.Sesiones, _escenario.Transaccion,
            _escenario.Proteccion, _escenario.Auditar, _escenario.Reloj);

        // Act
        var resultado = await caso.EjecutarAsync(usuario.Id, "ConductorRegistrado", _escenario.Administrador, EscenarioAplicacion.Ct);

        // Assert
        Assert.Null(resultado.Error);
        await _escenario.Sesiones.Received(1).RevocarUsuarioAsync(usuario.Id, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
        await AuditadoUnaVez(usuario);
    }

    /// <summary>Desactivar deja la cuenta inactiva y cierra todas sus sesiones (FR-019, FR-032).</summary>
    [Fact]
    public async Task Desactivar_revoca_sesiones_y_audita()
    {
        // Arrange
        var usuario = _escenario.Conductor();
        _escenario.Existen(usuario);
        var caso = new DesactivarUsuarioUseCase(_escenario.Usuarios, _escenario.Sesiones, _escenario.Transaccion,
            _escenario.Proteccion, _escenario.Auditar, _escenario.Reloj);

        // Act
        var resultado = await caso.EjecutarAsync(usuario.Id, "Motivo del administrador", _escenario.Administrador, EscenarioAplicacion.Ct);

        // Assert
        Assert.Null(resultado.Error);
        Assert.False(usuario.Activo);
        await _escenario.Sesiones.Received(1).RevocarUsuarioAsync(usuario.Id, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
        await AuditadoUnaVez(usuario);
    }

    /// <summary>Activar no crea sesiones: el usuario debe volver a iniciar sesión (FR-032).</summary>
    [Fact]
    public async Task Activar_reactiva_sin_tocar_sesiones()
    {
        // Arrange
        var usuario = _escenario.Conductor();
        usuario.Desactivar();
        _escenario.Existen(usuario);
        var caso = new ActivarUsuarioUseCase(_escenario.Usuarios, _escenario.Transaccion, _escenario.Auditar);

        // Act
        var resultado = await caso.EjecutarAsync(usuario.Id, _escenario.Administrador, EscenarioAplicacion.Ct);

        // Assert
        Assert.Null(resultado.Error);
        Assert.True(usuario.Activo);
        Assert.Empty(_escenario.Sesiones.ReceivedCalls());
        await AuditadoUnaVez(usuario);
    }
}
