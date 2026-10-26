using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Tests.Domain;

public sealed class UsuarioBloqueoTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 23, 10, 0, 0, TimeSpan.Zero);
    private static readonly ParametrosBloqueo Parametros = new(3, 15, [5, 15, 60]);

    [Fact]
    public void TercerFalloBloqueaYExitoReinicia()
    {
        var politica = new PoliticaBloqueo();
        Assert.False(politica.RegistrarFallo(Ahora, Parametros));
        Assert.False(politica.RegistrarFallo(Ahora, Parametros));
        Assert.True(politica.RegistrarFallo(Ahora, Parametros));
        Assert.Equal(Ahora.AddMinutes(5), politica.BloqueadoHasta);
        Assert.True(politica.EstaBloqueado(Ahora.AddMinutes(4)));
        Assert.False(politica.EstaBloqueado(Ahora.AddMinutes(5)));
        politica.RegistrarExito();
        Assert.Equal(0, politica.NivelBloqueo);
        Assert.Null(politica.BloqueadoHasta);
    }

    [Theory]
    [InlineData(900, true)]
    [InlineData(901, false)]
    public void VentanaTieneLimiteInclusivo(int segundos, bool bloquea)
    {
        var politica = new PoliticaBloqueo();
        politica.RegistrarFallo(Ahora, Parametros);
        politica.RegistrarFallo(Ahora, Parametros);
        Assert.Equal(bloquea, politica.RegistrarFallo(Ahora.AddSeconds(segundos), Parametros));
    }

    [Fact]
    public void BloqueosProgresanHastaElUltimoNivel()
    {
        var politica = new PoliticaBloqueo();
        var instante = Ahora;
        foreach (var minutos in new[] { 5, 15, 60, 60 })
        {
            for (var i = 0; i < 3; i++) politica.RegistrarFallo(instante, Parametros);
            Assert.Equal(instante.AddMinutes(minutos), politica.BloqueadoHasta);
            instante = politica.BloqueadoHasta!.Value;
        }
    }
}
