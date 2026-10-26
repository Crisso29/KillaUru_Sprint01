using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Application.Usuarios;
using NSubstitute;

namespace KillaUru.Identidad.Tests.Application;

/// <summary>
/// Ver correos de otras personas es un acceso administrativo a datos personales: se audita
/// antes de devolverlos y, si la auditoría falla, no se devuelve nada. Spec 003 · FR-030, FR-035.
/// </summary>
public sealed class ConsultasAdministrativasTests
{
    private readonly EscenarioAplicacion _escenario = new();
    private readonly ListarUsuariosUseCase _listar;

    public ConsultasAdministrativasTests()
    {
        _escenario.Usuarios.ListarAsync(Arg.Any<FiltrosUsuarios>(), Arg.Any<CancellationToken>()).Returns(new PaginaUsuarios([], 1, 50, 0));
        _listar = new ListarUsuariosUseCase(_escenario.Usuarios, _escenario.Auditar);
    }

    private Task LecturaAuditada(int veces) => _escenario.Auditoria.Received(veces).RegistrarLecturaAsync(
        AccionAuditada.AccesoAdministrativoDatos, Arg.Any<ActorAuditoria>(), "identidad.usuarios",
        Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>());

    /// <summary>La lista de usuarios del panel queda registrada como lectura (FR-030).</summary>
    [Fact]
    public async Task Listar_audita_la_lectura()
    {
        // Act
        var pagina = await _listar.EjecutarAsync(new(), _escenario.Administrador, EscenarioAplicacion.Ct);

        // Assert
        Assert.NotNull(pagina);
        await LecturaAuditada(1);
    }

    /// <summary>Resolver correos por id (para mostrar actores en Auditoría) también se audita (FR-035).</summary>
    [Fact]
    public async Task Resolver_correos_audita_la_lectura()
    {
        // Arrange
        var resolver = new ResolverCorreosUseCase(_listar);

        // Act
        var pagina = await resolver.EjecutarAsync([Guid.NewGuid()], _escenario.Administrador, EscenarioAplicacion.Ct);

        // Assert
        Assert.NotNull(pagina);
        await LecturaAuditada(1);
    }

    /// <summary>Sin rastro de auditoría no hay datos: la excepción llega al controlador (FR-030).</summary>
    [Fact]
    public async Task Sin_auditoria_no_se_devuelven_datos()
    {
        // Arrange
        _escenario.Auditoria.RegistrarLecturaAsync(Arg.Any<AccionAuditada>(), Arg.Any<ActorAuditoria>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>()).Returns(Task.FromException(new InvalidOperationException()));

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _listar.EjecutarAsync(new(), _escenario.Administrador, EscenarioAplicacion.Ct));
    }

    /// <summary>Una página de más de 200 se rechaza: evita volcar toda la tabla en una sola lectura (FR-035).</summary>
    [Fact]
    public async Task Pagina_de_mas_de_200_se_rechaza()
    {
        // Act
        var pagina = await _listar.EjecutarAsync(new(Tamano: 201), _escenario.Administrador, EscenarioAplicacion.Ct);

        // Assert
        Assert.Null(pagina);
    }
}
