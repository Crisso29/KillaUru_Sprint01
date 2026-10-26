using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Tests.Domain;

public sealed class HorasGuardadasTests
{
    [Theory]
    [InlineData(10, 59, 59, 10)]
    [InlineData(11, 0, 0, 11)]
    public void TruncaLaHora(int hora, int minuto, int segundo, int esperada)
    {
        var instante = new DateTimeOffset(2026, 10, 23, hora, minuto, segundo, TimeSpan.Zero);
        Assert.Equal(new DateTimeOffset(2026, 10, 23, esperada, 0, 0, TimeSpan.Zero), HorasGuardadas.ALaHora(instante));
    }

    [Fact]
    public void DiaEsLocalLima()
    {
        var instante = new DateTimeOffset(2026, 10, 23, 4, 59, 59, TimeSpan.Zero);
        Assert.Equal(new DateTimeOffset(2026, 10, 22, 5, 0, 0, TimeSpan.Zero), HorasGuardadas.AlDia(instante));
    }
}
