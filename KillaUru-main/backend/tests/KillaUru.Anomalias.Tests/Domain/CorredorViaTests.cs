using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Domain;

namespace KillaUru.Anomalias.Tests.Domain;

/// <summary>
/// La distancia a la PE-28A es perpendicular a la polilínea y el kilometraje recorre sus curvas;
/// nunca se sustituye la vía por una recta. Spec 004 · FR-003, FR-032.
/// </summary>
public sealed class CorredorViaTests
{
    /// <summary>La distancia perpendicular a la vía se mide con precisión de centímetros (FR-003).</summary>
    [Theory]
    [InlineData(49)]
    [InlineData(50)]
    [InlineData(51)]
    public void Mide_la_distancia_perpendicular(double metros)
    {
        // Arrange
        var punto = ReportesPrueba.Punto(ReportesPrueba.GradosDe(metros));

        // Act
        var distancia = ReportesPrueba.Corredor().DistanciaMetros(punto);

        // Assert
        Assert.Equal(metros, distancia, 5);
    }

    /// <summary>La franja de 50 m es inclusiva: a 51 m el evento está fuera del corredor (FR-003).</summary>
    [Theory]
    [InlineData(49, null)]
    [InlineData(50, null)]
    [InlineData(51, (int)MotivoDescarte.FueraDelCorredor)]
    public void Franja_de_cincuenta_metros_es_inclusiva(double metros, int? motivoEsperado)
    {
        // Arrange
        var corredor = ReportesPrueba.Corredor();
        var reporte = ReportesPrueba.Crear() with { Posicion = ReportesPrueba.Punto(ReportesPrueba.GradosDe(metros)) };

        // Act
        var motivo = new ReglasCalidad(new(), corredor).EvaluarIntrinseca(reporte, ReportesPrueba.Ahora);

        // Assert
        Assert.Equal((MotivoDescarte?)motivoEsperado, motivo);
    }

    /// <summary>Toda celda que toca la franja está indexada: el mapa y la app la encuentran.</summary>
    [Theory]
    [InlineData(49)]
    [InlineData(50)]
    public void Celdas_de_la_franja_estan_indexadas(double metros)
    {
        // Arrange
        var punto = ReportesPrueba.Punto(ReportesPrueba.GradosDe(metros));

        // Act
        var celdas = ReportesPrueba.Corredor().Celdas;

        // Assert
        Assert.Contains(Celda.De(punto).Id, celdas);
    }

    /// <summary>
    /// El km acumula cada segmento: en una curva A → B → C el km de C es mucho mayor que la línea
    /// recta A → C (FR-032).
    /// </summary>
    [Fact]
    public void Kilometraje_acumula_vertices_y_curvas()
    {
        // Arrange
        var (a, b, c) = (ReportesPrueba.Punto(), ReportesPrueba.Punto(0, 0.01), ReportesPrueba.Punto(0.001, 0));
        var corredor = new CorredorVia([a, b, c], 50);
        var kmEsperadoEnC = (GeodesiaLocal.Distancia(a, b) + GeodesiaLocal.Distancia(b, c)) / 1000;

        // Act
        var kmEnA = corredor.KmSobreVia(a);
        var kmEnB = corredor.KmSobreVia(b);
        var kmEnC = corredor.KmSobreVia(c);

        // Assert
        Assert.Equal(0, corredor.DistanciaMetros(a), 6);
        Assert.Equal(0, kmEnA, 6);
        Assert.Equal(GeodesiaLocal.Distancia(a, b) / 1000, kmEnB, 6);
        Assert.Equal(kmEsperadoEnC, kmEnC, 6);
        Assert.True(kmEnC > GeodesiaLocal.Distancia(a, c) / 1000 * 10);
    }
}
