using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Tests.Domain;

/// <summary>
/// D3: las fechas de Identidad se guardan con poca precisión para que no puedan cruzarse con la
/// hora exacta de un evento y reidentificar al conductor. Spec 003 · FR-039.
/// </summary>
public sealed class HorasGuardadasTests
{
    /// <summary>Se trunca hacia abajo: 10:59:59 queda en 10:00, nunca se redondea a 11:00.</summary>
    [Theory]
    [InlineData(10, 59, 59, 10)]
    [InlineData(11, 0, 0, 11)]
    public void Trunca_hacia_abajo_a_la_hora(int hora, int minuto, int segundo, int horaEsperada)
    {
        // Arrange
        var instante = new DateTimeOffset(2026, 10, 23, hora, minuto, segundo, TimeSpan.Zero);

        // Act
        var guardada = HorasGuardadas.ALaHora(instante);

        // Assert
        Assert.Equal(new DateTimeOffset(2026, 10, 23, horaEsperada, 0, 0, TimeSpan.Zero), guardada);
    }

    /// <summary>El día es el de Lima (UTC-5): 04:59 UTC del 23 todavía es el 22 en Perú.</summary>
    [Fact]
    public void Dia_se_calcula_en_hora_de_Lima()
    {
        // Arrange
        var instante = new DateTimeOffset(2026, 10, 23, 4, 59, 59, TimeSpan.Zero);

        // Act
        var dia = HorasGuardadas.AlDia(instante);

        // Assert
        Assert.Equal(new DateTimeOffset(2026, 10, 22, 5, 0, 0, TimeSpan.Zero), dia);
    }
}
