using KillaUru.Shared.Domain;

namespace KillaUru.Shared.Tests.Domain;

public sealed class CeldaTests
{
    // Un punto en el centro de una celda de 0,005° (≈ 550 m), lejos de los bordes.
    private static readonly Coordenada Centro = Coordenada.Crear(-13.2125, -74.2575)!.Value;

    [Fact]
    public void La_misma_coordenada_da_la_misma_celda() =>
        Assert.Equal(Celda.De(Centro), Celda.De(Coordenada.Crear(-13.2125, -74.2575)!.Value));

    [Fact]
    public void Un_punto_lejos_de_los_bordes_toca_una_sola_celda() =>
        Assert.Single(Celda.Alrededor(Centro, 100));

    [Fact]
    public void A_10_m_de_un_borde_toca_dos_celdas()
    {
        var cercaDeBorde = Coordenada.Crear(-13.2125, -74.25509)!.Value;   // ~10 m del borde en longitud -74.255

        Assert.Equal(2, Celda.Alrededor(cercaDeBorde, 100).Count);
    }

    [Fact]
    public void Cerca_de_una_esquina_toca_cuatro_celdas()
    {
        var cercaDeEsquina = Coordenada.Crear(-13.21009, -74.25509)!.Value;

        Assert.Equal(4, Celda.Alrededor(cercaDeEsquina, 100).Count);
    }

    [Fact]
    public void Alrededor_devuelve_celdas_en_orden_ascendente_sin_repetir()
    {
        var celdas = Celda.Alrededor(Coordenada.Crear(-13.21009, -74.25509)!.Value, 100);

        Assert.Equal(celdas.OrderBy(c => c.Id), celdas);
        Assert.Equal(celdas.Distinct().Count(), celdas.Count);
    }

    [Theory]
    [InlineData(-13.2125, -74.2575)]
    [InlineData(-14.0, -76.2)]
    [InlineData(0.0, 0.0)]
    [InlineData(-13.21, -74.255)]
    public void Limites_contienen_la_coordenada_que_genero_la_celda(double lat, double lon)
    {
        var limites = Celda.De(Coordenada.Crear(lat, lon)!.Value).Limites();

        Assert.InRange(lat, limites.MinLatitud, limites.MaxLatitud);
        Assert.InRange(lon, limites.MinLongitud, limites.MaxLongitud);
        Assert.Equal(0.005, limites.MaxLatitud - limites.MinLatitud, 9);
    }
}
