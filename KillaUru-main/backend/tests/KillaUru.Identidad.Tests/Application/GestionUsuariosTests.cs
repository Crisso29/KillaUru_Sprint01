using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Application.Usuarios;
using KillaUru.Identidad.Domain;
using NSubstitute;
namespace KillaUru.Identidad.Tests.Application;

public sealed class GestionUsuariosTests
{
    [Fact]
    public async Task Ultimo_administrador_y_accion_sobre_si_mismo_se_rechazan()
    {
        var e = new EscenarioAplicacion();
        var usuario = Usuario.Invitar(Correo.Crear("admin@example.com")!, RolUsuario.Administrador, e.Reloj.GetUtcNow());
        e.Usuarios.AdministradoresAsync(Arg.Any<CancellationToken>()).Returns(new[] { usuario });
        var proteccion = new ProteccionAdministradores(e.Usuarios);
        Assert.Equal("accion_sobre_si_mismo", await proteccion.ValidarRetiroAsync(usuario, usuario.Id, TestContext.Current.CancellationToken));
        Assert.Equal("ultimo_administrador", await proteccion.ValidarRetiroAsync(usuario, Guid.NewGuid(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Rol_y_estado_revocan_y_auditan_sin_duplicar_metricas()
    {
        var e = new EscenarioAplicacion();
        var u = Usuario.RegistrarConductor(Correo.Crear("a@example.com")!, "hash", e.Reloj.GetUtcNow());
        e.Usuarios.PorIdAsync(u.Id, Arg.Any<CancellationToken>()).Returns(u);
        var actor = new ActorIdentidad(Guid.NewGuid(), "Administrador");
        var proteccion = new ProteccionAdministradores(e.Usuarios);
        var cambiar = new CambiarRolUseCase(e.Usuarios, e.Sesiones, e.Transaccion, proteccion, e.Auditar, e.Reloj);
        Assert.Null((await cambiar.EjecutarAsync(u.Id, "ConductorRegistrado", actor, TestContext.Current.CancellationToken)).Error);
        var desactivar = new DesactivarUsuarioUseCase(e.Usuarios, e.Sesiones, e.Transaccion, proteccion, e.Auditar, e.Reloj);
        Assert.Null((await desactivar.EjecutarAsync(u.Id, "Motivo con correo que no debe persistir", actor, TestContext.Current.CancellationToken)).Error);
        Assert.False(u.Activo);
        var activar = new ActivarUsuarioUseCase(e.Usuarios, e.Transaccion, e.Auditar);
        Assert.Null((await activar.EjecutarAsync(u.Id, actor, TestContext.Current.CancellationToken)).Error);
        Assert.True(u.Activo);
        await e.Sesiones.Received(2).RevocarUsuarioAsync(u.Id, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
        await e.Auditoria.Received(3).RegistrarAsync(Arg.Any<AccionAuditada>(), Arg.Any<ActorAuditoria>(),
            u.Id.ToString(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Lista_e_ids_no_devuelven_datos_si_falla_auditoria()
    {
        var e = new EscenarioAplicacion();
        e.Usuarios.ListarAsync(Arg.Any<FiltrosUsuarios>(), Arg.Any<CancellationToken>()).Returns(new PaginaUsuarios([], 1, 50, 0));
        var listar = new ListarUsuariosUseCase(e.Usuarios, e.Auditar);
        var resolver = new ResolverCorreosUseCase(listar);
        var actor = new ActorIdentidad(Guid.NewGuid(), "Administrador");
        Assert.NotNull(await listar.EjecutarAsync(new(), actor, TestContext.Current.CancellationToken));
        Assert.NotNull(await resolver.EjecutarAsync([Guid.NewGuid()], actor, TestContext.Current.CancellationToken));
        await e.Auditoria.Received(2).RegistrarLecturaAsync(AccionAuditada.AccesoAdministrativoDatos, Arg.Any<ActorAuditoria>(),
            "identidad.usuarios", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>());
        e.Auditoria.RegistrarLecturaAsync(Arg.Any<AccionAuditada>(), Arg.Any<ActorAuditoria>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>()).Returns(Task.FromException(new InvalidOperationException()));
        await Assert.ThrowsAsync<InvalidOperationException>(() => listar.EjecutarAsync(new(), actor, TestContext.Current.CancellationToken));
        Assert.Null(await listar.EjecutarAsync(new(Tamano: 201), actor, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Invitacion_rechaza_duplicados_y_reenvio_invalida_anterior()
    {
        var e = new EscenarioAplicacion();
        var actor = new ActorIdentidad(Guid.NewGuid(), "Administrador");
        var crear = new CrearUsuarioUseCase(e.Usuarios, e.Transaccion, e.Enlaces, e.Auditar, e.Reloj);
        var creado = await crear.EjecutarAsync("nuevo@example.com", "Administrador", actor, TestContext.Current.CancellationToken);
        Assert.Equal("InvitacionPendiente", creado.Usuario!.Estado);
        var u = Usuario.Invitar(Correo.Crear("nuevo@example.com")!, RolUsuario.Administrador, e.Reloj.GetUtcNow());
        e.Usuarios.PorCorreoAsync(u.Correo.Valor, Arg.Any<CancellationToken>()).Returns(u);
        Assert.Equal("correo_en_uso", (await crear.EjecutarAsync(u.Correo.Valor, "Administrador", actor, TestContext.Current.CancellationToken)).Error);
        e.Usuarios.PorIdAsync(u.Id, Arg.Any<CancellationToken>()).Returns(u);
        var reenviar = new ReenviarCorreoUsuarioUseCase(e.Usuarios, e.Transaccion, e.Enlaces, e.Auditar);
        Assert.Null((await reenviar.EjecutarAsync(u.Id, actor, TestContext.Current.CancellationToken)).Error);
        await e.Tokens.Received(1).InvalidarAsync(u.Id, PropositoToken.Invitacion, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }
}
