using KillaUru.Identidad.Domain;
using NSubstitute;

namespace KillaUru.Identidad.Tests.Application;

/// <summary>
/// Un administrador no puede retirarse a sí mismo ni dejar el sistema sin administradores activos:
/// nadie podría volver a entrar al panel. Spec 003 · FR-033.
/// </summary>
public sealed class ProteccionAdministradoresTests
{
    private readonly EscenarioAplicacion _escenario = new();

    /// <summary>Desactivarse o quitarse el rol a uno mismo se rechaza siempre.</summary>
    [Fact]
    public async Task Accion_sobre_si_mismo_se_rechaza()
    {
        // Arrange
        var admin = _escenario.AdministradorRegistrado("admin@example.com");
        _escenario.Usuarios.AdministradoresAsync(Arg.Any<CancellationToken>()).Returns([admin, _escenario.AdministradorRegistrado("otro@example.com")]);

        // Act
        var error = await _escenario.Proteccion.ValidarRetiroAsync(admin, actor: admin.Id, EscenarioAplicacion.Ct);

        // Assert
        Assert.Equal("accion_sobre_si_mismo", error);
    }

    /// <summary>Retirar al único administrador activo dejaría el panel sin dueño.</summary>
    [Fact]
    public async Task Ultimo_administrador_no_puede_retirarse()
    {
        // Arrange
        var unico = _escenario.AdministradorRegistrado("admin@example.com");
        _escenario.Usuarios.AdministradoresAsync(Arg.Any<CancellationToken>()).Returns([unico]);

        // Act
        var error = await _escenario.Proteccion.ValidarRetiroAsync(unico, actor: Guid.NewGuid(), EscenarioAplicacion.Ct);

        // Assert
        Assert.Equal("ultimo_administrador", error);
    }
}
