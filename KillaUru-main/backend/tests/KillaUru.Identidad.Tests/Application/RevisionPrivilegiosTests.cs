using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Privilegios;
using KillaUru.Identidad.Domain;
using NSubstitute;

namespace KillaUru.Identidad.Tests.Application;

/// <summary>
/// Revisión mensual de privilegios: cada administrador activo debe aparecer exactamente una vez
/// (confirmado o retirado); retirar desactiva y cierra sesiones. Spec 003 · FR-033, FR-034.
/// </summary>
public sealed class RevisionPrivilegiosTests
{
    private readonly EscenarioAplicacion _escenario = new();
    private readonly Usuario _revisor;
    private readonly Usuario _otro;
    private readonly RegistrarRevisionUseCase _caso;

    public RevisionPrivilegiosTests()
    {
        _revisor = _escenario.AdministradorRegistrado("a@example.com");
        _otro = _escenario.AdministradorRegistrado("b@example.com");
        _escenario.Usuarios.AdministradoresAsync(Arg.Any<CancellationToken>()).Returns([_revisor, _otro]);
        _caso = new RegistrarRevisionUseCase(_escenario.Usuarios, _escenario.Revisiones, _escenario.Sesiones,
            _escenario.Transaccion, _escenario.Auditar, _escenario.Reloj);
    }

    private ActorIdentidad Revisor => new(_revisor.Id, "Administrador");

    /// <summary>Una revisión que omite o repite administradores no es una revisión completa.</summary>
    [Fact]
    public async Task Conjunto_incompleto_o_repetido_se_rechaza()
    {
        // Act
        var faltaUno = await _caso.EjecutarAsync([_revisor.Id], [], Revisor, EscenarioAplicacion.Ct);
        var repetido = await _caso.EjecutarAsync([_revisor.Id, _revisor.Id], [_otro.Id], Revisor, EscenarioAplicacion.Ct);

        // Assert
        Assert.Equal("revision_incompleta", faltaUno.Error);
        Assert.Equal("revision_incompleta", repetido.Error);
    }

    /// <summary>Quien revisa no puede retirarse a sí mismo (FR-033).</summary>
    [Fact]
    public async Task Revisor_no_puede_retirarse()
    {
        // Act
        var resultado = await _caso.EjecutarAsync([_otro.Id], [_revisor.Id], Revisor, EscenarioAplicacion.Ct);

        // Assert
        Assert.Equal("accion_sobre_si_mismo", resultado.Error);
    }

    /// <summary>Retirar a todos dejaría el panel sin administradores (FR-033).</summary>
    [Fact]
    public async Task Retirar_a_todos_se_rechaza()
    {
        // Arrange
        var ajeno = new ActorIdentidad(Guid.NewGuid(), "Administrador");

        // Act
        var resultado = await _caso.EjecutarAsync([], [_revisor.Id, _otro.Id], ajeno, EscenarioAplicacion.Ct);

        // Assert
        Assert.Equal("ultimo_administrador", resultado.Error);
    }

    /// <summary>Retirar desactiva, cierra las sesiones del retirado y guarda la constancia (FR-034).</summary>
    [Fact]
    public async Task Revision_valida_retira_y_guarda_constancia()
    {
        // Act
        var resultado = await _caso.EjecutarAsync([_revisor.Id], [_otro.Id], Revisor, EscenarioAplicacion.Ct);

        // Assert
        Assert.Null(resultado.Error);
        Assert.False(_otro.Activo);
        await _escenario.Sesiones.Received(1).RevocarUsuarioAsync(_otro.Id, _escenario.Reloj.GetUtcNow(), EscenarioAplicacion.Ct);
        _escenario.Revisiones.Received(1).Agregar(Arg.Any<RevisionPrivilegios>());
    }

    /// <summary>Consultar el estado de la revisión muestra correos de administradores: se audita (FR-034).</summary>
    [Fact]
    public async Task Consultar_revision_audita_la_lectura()
    {
        // Arrange
        var consulta = new ConsultarRevisionUseCase(_escenario.Usuarios, _escenario.Revisiones, _escenario.Auditar, _escenario.Reloj);

        // Act
        await consulta.EjecutarAsync(Revisor, EscenarioAplicacion.Ct);

        // Assert
        await _escenario.Auditoria.Received(1).RegistrarLecturaAsync(AccionAuditada.AccesoAdministrativoDatos,
            Arg.Any<ActorAuditoria>(), "identidad.revision_privilegios", Arg.Any<IReadOnlyDictionary<string, object?>>(), EscenarioAplicacion.Ct);
    }
}
