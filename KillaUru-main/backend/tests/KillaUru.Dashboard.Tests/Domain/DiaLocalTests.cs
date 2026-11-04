using KillaUru.Dashboard.Domain;

namespace KillaUru.Dashboard.Tests.Domain;

/// <summary>Conversión de UTC a día local Lima y detección de huecos (SC-001, SC-002).</summary>
public sealed class DiaLocalTests
{
    [Fact]
    public void Hoy_convierte_utc_a_zona_local()
    {
        // Arrange
        var ahora = new DateTimeOffset(2026, 10, 21, 3, 0, 0, TimeSpan.Zero);

        // Act
        var hoy = DiaLocal.Hoy(ahora, desfaseHoras: -5);

        // Assert
        Assert.Equal(new DateOnly(2026, 10, 20), hoy);
    }

    [Fact]
    public void Hoy_en_nueva_medianoche_local_devuelve_dia_correcto()
    {
        // Arrange
        var ahora = new DateTimeOffset(2026, 10, 21, 5, 0, 0, TimeSpan.Zero); // 00:00 Lima

        // Act
        var hoy = DiaLocal.Hoy(ahora, desfaseHoras: -5);

        // Assert
        Assert.Equal(new DateOnly(2026, 10, 21), hoy);
    }

    [Fact]
    public void DiasFaltantes_devuelve_huecos_del_mas_antiguo_al_mas_reciente()
    {
        // Arrange
        var hoy       = new DateOnly(2026, 10, 23);
        var guardados = new HashSet<DateOnly> { hoy.AddDays(-2) };

        // Act
        var faltantes = DiaLocal.DiasFaltantes(guardados, hoy, recuperables: 2);

        // Assert
        Assert.Equal([hoy.AddDays(-1)], faltantes);
    }

    [Fact]
    public void DiasFaltantes_sin_guardados_devuelve_todos_los_recuperables()
    {
        // Arrange
        var hoy = new DateOnly(2026, 10, 23);

        // Act
        var faltantes = DiaLocal.DiasFaltantes(new HashSet<DateOnly>(), hoy, recuperables: 2);

        // Assert
        Assert.Equal([hoy.AddDays(-2), hoy.AddDays(-1)], faltantes);
    }

    [Fact]
    public void DiasFaltantes_no_incluye_dias_fuera_de_ventana_recuperable()
    {
        // Arrange
        var hoy = new DateOnly(2026, 10, 23);

        // Act
        var faltantes = DiaLocal.DiasFaltantes(new HashSet<DateOnly>(), hoy, recuperables: 2);

        // Assert — solo D-1 y D-2; D-3 no se toca
        Assert.Equal(2, faltantes.Count);
        Assert.DoesNotContain(hoy.AddDays(-3), faltantes);
    }

    [Fact]
    public void DiasFaltantes_todos_guardados_devuelve_lista_vacia()
    {
        // Arrange
        var hoy       = new DateOnly(2026, 10, 23);
        var guardados = new HashSet<DateOnly> { hoy.AddDays(-2), hoy.AddDays(-1) };

        // Act
        var faltantes = DiaLocal.DiasFaltantes(guardados, hoy, recuperables: 2);

        // Assert
        Assert.Empty(faltantes);
    }
}
