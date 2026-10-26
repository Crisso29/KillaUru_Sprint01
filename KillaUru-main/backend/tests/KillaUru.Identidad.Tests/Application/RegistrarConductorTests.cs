using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application.Cuenta;
using KillaUru.Identidad.Domain;
using NSubstitute;

namespace KillaUru.Identidad.Tests.Application;

/// <summary>
/// Registro público del conductor: valida contraseña y aceptación de las versiones vigentes antes
/// de tocar la base, audita dentro de la transacción y envía la verificación solo tras confirmar.
/// Spec 003 · FR-001, FR-002, FR-005, FR-021, FR-038.
/// </summary>
public sealed class RegistrarConductorTests
{
    private readonly EscenarioAplicacion _escenario = new();

    private RegistrarConductorUseCase Caso() => new(_escenario.Usuarios, _escenario.Hash, _escenario.Transaccion,
        _escenario.Emision, _escenario.Enlaces, _escenario.Auditar, _escenario.Opciones, _escenario.Reloj);

    /// <summary>
    /// Contraseña de menos de 10 caracteres (FR-005) o versión aceptada distinta de la vigente
    /// (FR-002) se rechazan sin abrir la transacción.
    /// </summary>
    [Theory]
    [InlineData("123456789", "1.2", "datos_invalidos")]
    [InlineData("1234567890", "1.1", "aceptacion_requerida")]
    [InlineData("1234567890", null, "aceptacion_requerida")]
    public async Task Entrada_invalida_se_rechaza_sin_persistir(string contrasena, string? versionPolitica, string errorEsperado)
    {
        // Arrange
        var entrada = new RegistroEntrada("a@example.com", contrasena, versionPolitica, "1.2");

        // Act
        var resultado = await Caso().EjecutarAsync(entrada, EscenarioAplicacion.Ct);

        // Assert
        Assert.Equal(errorEsperado, resultado.Error);
        Assert.Empty(_escenario.Usuarios.ReceivedCalls());
    }

    /// <summary>El registro válido crea un conductor sin verificar y audita UsuarioCreado (FR-001, FR-038).</summary>
    [Fact]
    public async Task Registro_valido_crea_conductor_y_audita()
    {
        // Arrange
        var entrada = new RegistroEntrada("a@example.com", "1234567890", "1.2", "1.2");

        // Act
        var resultado = await Caso().EjecutarAsync(entrada, EscenarioAplicacion.Ct);

        // Assert
        Assert.Null(resultado.Error);
        _escenario.Usuarios.Received(1).Agregar(Arg.Is<Usuario>(u => u.Rol == RolUsuario.ConductorRegistrado && !u.CorreoVerificado));
        await _escenario.Auditoria.Received(1).RegistrarAsync(AccionAuditada.UsuarioCreado, ActorAuditoria.Sistema,
            Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// El correo de verificación se programa para después del commit: si la transacción se
    /// deshace, nadie recibe un enlace de una cuenta que no existe (FR-021).
    /// </summary>
    [Fact]
    public async Task Verificacion_se_envia_solo_despues_de_confirmar()
    {
        // Arrange
        var entrada = new RegistroEntrada("a@example.com", "1234567890", "1.2", "1.2");

        // Act
        await Caso().EjecutarAsync(entrada, EscenarioAplicacion.Ct);

        // Assert
        _escenario.Transaccion.Received(1).AlConfirmar(Arg.Any<Func<Task>>());
        await _escenario.Correo.DidNotReceiveWithAnyArgs().VerificacionAsync(default!, default!, EscenarioAplicacion.Ct);
    }

    /// <summary>Se guardan las dos constancias: política de privacidad y términos de uso (FR-002).</summary>
    [Fact]
    public async Task Registro_guarda_ambos_consentimientos()
    {
        // Arrange
        var entrada = new RegistroEntrada("a@example.com", "1234567890", "1.2", "1.2");

        // Act
        await Caso().EjecutarAsync(entrada, EscenarioAplicacion.Ct);

        // Assert
        await _escenario.Usuarios.Received(1).AceptarAsync(Arg.Is<Consentimiento>(c => c.Documento == DocumentoLegal.PoliticaPrivacidad), Arg.Any<CancellationToken>());
        await _escenario.Usuarios.Received(1).AceptarAsync(Arg.Is<Consentimiento>(c => c.Documento == DocumentoLegal.TerminosUso), Arg.Any<CancellationToken>());
    }
}
