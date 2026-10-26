using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Tests.Domain;

public sealed class SesionRenovacionTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 23, 10, 17, 43, TimeSpan.Zero);
    private static SesionRenovacion Crear() => SesionRenovacion.Crear(Guid.NewGuid(), Guid.NewGuid(), [1], Ahora, Ahora.AddDays(30), null);

    [Theory]
    [InlineData(59, null)]
    [InlineData(60, null)]
    [InlineData(61, "sesion_invalida")]
    public void GraciaInclusiva(int segundos, string? error)
    {
        var sesion = Crear();
        sesion.Usar(Ahora);
        Assert.Equal(Ahora, sesion.UsadoUtc);
        Assert.Equal(error, sesion.Validar(Ahora.AddSeconds(segundos), true, 60));
    }

    [Fact]
    public void RevocacionVencimientoYDesactivacion()
    {
        var sesion = Crear();
        Assert.Null(sesion.Validar(Ahora, true, 60));
        Assert.Equal("cuenta_desactivada", sesion.Validar(Ahora, false, 60));
        Assert.Equal("sesion_invalida", sesion.Validar(sesion.ExpiraUtc, true, 60));
        sesion.Revocar(Ahora);
        Assert.Equal(HorasGuardadas.ALaHora(Ahora), sesion.RevocadoUtc);
        Assert.Equal("sesion_invalida", sesion.Validar(Ahora, true, 60));
    }
}
