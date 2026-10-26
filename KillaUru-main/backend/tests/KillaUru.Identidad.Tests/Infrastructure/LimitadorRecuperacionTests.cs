using KillaUru.Identidad.Application;
using KillaUru.Identidad.Infrastructure.Seguridad;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace KillaUru.Identidad.Tests.Infrastructure;

/// <summary>
/// Las solicitudes de recuperación se limitan a 3 por correo y 10 por IP cada hora, para que nadie
/// inunde un buzón ni recorra correos desde una sola IP. Spec 003 · FR-022.
/// </summary>
public sealed class LimitadorRecuperacionTests
{
    private readonly FakeTimeProvider _reloj = new();
    private readonly LimitadorRecuperacionEnMemoria _limitador;

    public LimitadorRecuperacionTests() =>
        _limitador = new LimitadorRecuperacionEnMemoria(Options.Create(new IdentidadOptions()), _reloj);

    /// <summary>El cuarto pedido al mismo correo se rechaza aunque venga de otra IP.</summary>
    [Fact]
    public void Cuarto_pedido_al_mismo_correo_se_rechaza()
    {
        // Arrange
        for (var pedido = 0; pedido < 3; pedido++) _limitador.Admitir("a@example.com", "ip-1");

        // Act
        var admitido = _limitador.Admitir("a@example.com", "ip-2");

        // Assert
        Assert.False(admitido);
    }

    /// <summary>El pedido 11 desde la misma IP se rechaza aunque sea para otro correo.</summary>
    [Fact]
    public void Undecimo_pedido_desde_la_misma_ip_se_rechaza()
    {
        // Arrange
        for (var pedido = 0; pedido < 10; pedido++) _limitador.Admitir($"{pedido}@example.com", "ip-1");

        // Act
        var admitido = _limitador.Admitir("nuevo@example.com", "ip-1");

        // Assert
        Assert.False(admitido);
    }

    /// <summary>Pasada una hora, el límite se libera.</summary>
    [Fact]
    public void Despues_de_una_hora_se_libera()
    {
        // Arrange
        for (var pedido = 0; pedido < 3; pedido++) _limitador.Admitir("a@example.com", "ip-1");
        _reloj.Advance(TimeSpan.FromHours(1));

        // Act
        var admitido = _limitador.Admitir("a@example.com", "ip-1");

        // Assert
        Assert.True(admitido);
    }
}
