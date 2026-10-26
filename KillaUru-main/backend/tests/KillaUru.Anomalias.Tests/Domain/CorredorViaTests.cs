using KillaUru.Anomalias.Domain;
namespace KillaUru.Anomalias.Tests.Domain;
/// <summary>Distancia perpendicular y recorrido por una curva, sin sustituir la vía por una recta.</summary>
public sealed class CorredorViaTests
{
    [Theory]
    [InlineData(49, null)]
    [InlineData(50, null)]
    [InlineData(51, (int)MotivoDescarte.FueraDelCorredor)]
    public void Franja(double metros, int? esperado)
    {
        var p = ReportesPrueba.Punto(metros / 6371008.8 * 180 / Math.PI);
        var corredor = ReportesPrueba.Corredor();
        Assert.Equal(metros, corredor.DistanciaMetros(p), 5);
        Assert.Equal((MotivoDescarte?)esperado, new ReglasCalidad(new(), corredor).EvaluarIntrinseca(ReportesPrueba.Crear() with { Posicion = p }, ReportesPrueba.Ahora));
        if (metros <= 50) Assert.Contains(KillaUru.Shared.Domain.Celda.De(p).Id, corredor.Celdas);
    }
    [Fact]
    public void Kilometraje_acumula_vertices_y_curvas()
    {
        var a = ReportesPrueba.Punto(); var b = ReportesPrueba.Punto(0, 0.01); var c = ReportesPrueba.Punto(0.001, 0);
        var corredor = new CorredorVia([a, b, c], 50);
        Assert.Equal(0, corredor.DistanciaMetros(a), 6);
        Assert.Equal(0, corredor.KmSobreVia(a), 6);
        Assert.Equal(GeodesiaLocal.Distancia(a, b) / 1000, corredor.KmSobreVia(b), 6);
        Assert.Equal((GeodesiaLocal.Distancia(a, b) + GeodesiaLocal.Distancia(b, c)) / 1000, corredor.KmSobreVia(c), 6);
        Assert.True(corredor.KmSobreVia(c) > GeodesiaLocal.Distancia(a, c) / 1000 * 10);
    }
}
