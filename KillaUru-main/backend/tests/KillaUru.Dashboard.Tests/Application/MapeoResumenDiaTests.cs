using KillaUru.Anomalias.Contracts.Lecturas;
using KillaUru.Dashboard.Application;

namespace KillaUru.Dashboard.Tests.Application;

/// <summary>T026: MapeoResumenDia mapea ResumenDia + conductoresActivos → DiaIndicadores.</summary>
public sealed class MapeoResumenDiaTests
{
    [Fact]
    public void Desde_copia_todos_los_campos_correctamente()
    {
        // Arrange
        var dia     = new DateOnly(2026, 10, 20);
        var resumen = new ResumenDia(dia,
            EventosRecibidos:    500L,
            DispositivosDistintos: 12L,
            TramosConReportes7d: 7,
            TramosTotales:       237,
            Confirmaciones:      18L,
            PendientesExpiradas: 6L,
            Expiraciones:        3L,
            Renacimientos:       2L);

        // Act
        var d = MapeoResumenDia.Desde(resumen, conductoresActivos: 42);

        // Assert
        Assert.Equal(dia,  d.Dia);
        Assert.Equal(500L, d.EventosRecibidos);
        Assert.Equal(42L,  d.ConductoresActivos);
        Assert.Equal(12L,  d.DispositivosDistintos);
        Assert.Equal(7,    d.TramosConReportes7d);
        Assert.Equal(237,  d.TramosTotales);
        Assert.Equal(18L,  d.Confirmaciones);
        Assert.Equal(6L,   d.PendientesExpiradas);
        Assert.Equal(3L,   d.Expiraciones);
        Assert.Equal(2L,   d.Renacimientos);
    }

    [Fact]
    public void ComoFilas_genera_exactamente_9_indicadores_en_orden()
    {
        // Arrange
        var dia     = new DateOnly(2026, 10, 20);
        var resumen = new ResumenDia(dia, 100L, 1L, 10, 20, 5L, 2L, 1L, 1L);
        var d       = MapeoResumenDia.Desde(resumen, conductoresActivos: 5);

        // Act
        var filas = d.ComoFilas().ToList();

        // Assert
        Assert.Equal(9, filas.Count);
        Assert.True(filas.Select(f => (int)f.Ind).SequenceEqual([1, 2, 3, 4, 5, 6, 7, 8, 9]));
    }
}
