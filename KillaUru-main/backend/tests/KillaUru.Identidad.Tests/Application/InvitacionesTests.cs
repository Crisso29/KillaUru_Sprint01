using KillaUru.Identidad.Adapters.Consola;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Application.Usuarios;
using KillaUru.Identidad.Domain;
using NSubstitute;

namespace KillaUru.Identidad.Tests.Application;

/// <summary>
/// El administrador crea cuentas por invitación (enlace de 24 h para fijar la contraseña) y puede
/// reenviarla; el primer administrador nace de un comando de consola. Spec 003 · FR-031, FR-032, FR-043.
/// </summary>
public sealed class InvitacionesTests
{
    private readonly EscenarioAplicacion _escenario = new();

    private CrearUsuarioUseCase Crear() => new(_escenario.Usuarios, _escenario.Transaccion, _escenario.Enlaces, _escenario.Auditar, _escenario.Reloj);

    /// <summary>La cuenta nueva queda pendiente hasta que la persona fije su contraseña (FR-031).</summary>
    [Fact]
    public async Task Crear_usuario_deja_la_invitacion_pendiente()
    {
        // Act
        var resultado = await Crear().EjecutarAsync("nuevo@example.com", "Administrador", _escenario.Administrador, EscenarioAplicacion.Ct);

        // Assert
        Assert.Equal("InvitacionPendiente", resultado.Usuario!.Estado);
    }

    /// <summary>El correo es único: invitar a uno existente responde correo_en_uso (FR-006).</summary>
    [Fact]
    public async Task Correo_existente_se_rechaza()
    {
        // Arrange
        var existente = _escenario.AdministradorRegistrado("nuevo@example.com");
        _escenario.Usuarios.PorCorreoAsync(existente.Correo.Valor, Arg.Any<CancellationToken>()).Returns(existente);

        // Act
        var resultado = await Crear().EjecutarAsync(existente.Correo.Valor, "Administrador", _escenario.Administrador, EscenarioAplicacion.Ct);

        // Assert
        Assert.Equal("correo_en_uso", resultado.Error);
    }

    /// <summary>Reenviar invalida el enlace anterior: solo el último sirve (FR-032).</summary>
    [Fact]
    public async Task Reenviar_invalida_la_invitacion_anterior()
    {
        // Arrange
        var invitado = _escenario.AdministradorRegistrado("nuevo@example.com");
        _escenario.Existen(invitado);
        var reenviar = new ReenviarCorreoUsuarioUseCase(_escenario.Usuarios, _escenario.Transaccion, _escenario.Enlaces, _escenario.Auditar);

        // Act
        var resultado = await reenviar.EjecutarAsync(invitado.Id, _escenario.Administrador, EscenarioAplicacion.Ct);

        // Assert
        Assert.Null(resultado.Error);
        await _escenario.Tokens.Received(1).InvalidarAsync(invitado.Id, PropositoToken.Invitacion, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// El enlace del primer administrador sale solo por la consola del comando, nunca por correo
    /// ni por logs, porque aún no hay nadie que pueda recibirlo de forma segura (FR-043).
    /// </summary>
    [Fact]
    public async Task Primer_administrador_imprime_el_enlace_solo_en_la_consola()
    {
        // Arrange
        _escenario.Usuarios.ListarAsync(Arg.Any<FiltrosUsuarios>(), Arg.Any<CancellationToken>()).Returns(new PaginaUsuarios([], 1, 1, 0));
        var comando = new CrearAdminComando(new CrearAdministradorInicialUseCase(_escenario.Usuarios, _escenario.Transaccion,
            _escenario.Enlaces, _escenario.Auditar, _escenario.Reloj));
        using var consola = new StringWriter();

        // Act
        var codigoSalida = await comando.EjecutarAsync("admin@example.com", consola, EscenarioAplicacion.Ct);

        // Assert
        Assert.Equal(0, codigoSalida);
        Assert.Contains("#token=secreto-sintetico", consola.ToString());
        Assert.Empty(_escenario.Correo.ReceivedCalls());
        _escenario.Transaccion.DidNotReceive().AlConfirmar(Arg.Any<Func<Task>>());
    }

    /// <summary>El comando no sirve para crear un segundo administrador por la puerta trasera (FR-043).</summary>
    [Fact]
    public async Task Primer_administrador_no_se_crea_si_ya_hay_uno()
    {
        // Arrange
        _escenario.Usuarios.ListarAsync(Arg.Any<FiltrosUsuarios>(), Arg.Any<CancellationToken>()).Returns(new PaginaUsuarios([], 1, 1, 1));
        var caso = new CrearAdministradorInicialUseCase(_escenario.Usuarios, _escenario.Transaccion, _escenario.Enlaces, _escenario.Auditar, _escenario.Reloj);

        // Act
        var resultado = await caso.EjecutarAsync("otro@example.com", EscenarioAplicacion.Ct);

        // Assert
        Assert.Equal("administrador_existente", resultado.Error);
    }
}
