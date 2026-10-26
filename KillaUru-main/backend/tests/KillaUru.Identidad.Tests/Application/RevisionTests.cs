using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Privilegios;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Domain;
using NSubstitute;
namespace KillaUru.Identidad.Tests.Application;

/// <summary>La revisión rechaza conjuntos parciales y preserva al administrador que la realiza.</summary>
public sealed class RevisionTests
{
    [Fact]
    public async Task Revision_valida_conjunto_autoproteccion_y_revocacion()
    {
        var e = new EscenarioAplicacion();
        var a = Usuario.Invitar(Correo.Crear("a@example.com")!, RolUsuario.Administrador, e.Reloj.GetUtcNow());
        var b = Usuario.Invitar(Correo.Crear("b@example.com")!, RolUsuario.Administrador, e.Reloj.GetUtcNow());
        e.Usuarios.AdministradoresAsync(Arg.Any<CancellationToken>()).Returns(new[] { a, b });
        var revisiones = Substitute.For<IRevisionRepositorio>();
        var caso = new RegistrarRevisionUseCase(e.Usuarios, revisiones, e.Sesiones, e.Transaccion, e.Auditar, e.Reloj);
        var actor = new ActorIdentidad(a.Id, "Administrador");
        var ct = TestContext.Current.CancellationToken;
        Assert.Equal("revision_incompleta", (await caso.EjecutarAsync([a.Id], [], actor, ct)).Error);
        Assert.Equal("revision_incompleta", (await caso.EjecutarAsync([a.Id, a.Id], [b.Id], actor, ct)).Error);
        Assert.Equal("accion_sobre_si_mismo", (await caso.EjecutarAsync([b.Id], [a.Id], actor, ct)).Error);
        Assert.Equal("ultimo_administrador", (await caso.EjecutarAsync([], [a.Id, b.Id], new(Guid.NewGuid(), "Administrador"), ct)).Error);
        Assert.Null((await caso.EjecutarAsync([a.Id], [b.Id], actor, ct)).Error);
        Assert.False(b.Activo);
        await e.Sesiones.Received(1).RevocarUsuarioAsync(b.Id, e.Reloj.GetUtcNow(), ct);
        revisiones.Received(1).Agregar(Arg.Any<RevisionPrivilegios>());
        var consulta = new ConsultarRevisionUseCase(e.Usuarios, revisiones, e.Auditar, e.Reloj);
        await consulta.EjecutarAsync(actor, ct);
        await e.Auditoria.Received(1).RegistrarLecturaAsync(AccionAuditada.AccesoAdministrativoDatos,
            Arg.Any<ActorAuditoria>(), "identidad.revision_privilegios", Arg.Any<IReadOnlyDictionary<string, object?>>(), ct);
    }
}
