using KillaUru.Mapa.Domain;

namespace KillaUru.Mapa.Tests.Domain;

/// <summary>
/// ZonaDemo clasifica celdas como Dentro, Borde o Fuera del polígono demo, y verifica puntos.
/// Spec 006 · RN-08; diseño-interno §4.
/// </summary>
public sealed class ZonaDemoTests
{
    // Polígono: cuadrado de 2° de lado centrado en (0,0) → vértices en ±1
    // [lon, lat] como GeoJSON
    private static readonly double[][] PoligonoUnitario =
    [
        [-1.0, -1.0],
        [ 1.0, -1.0],
        [ 1.0,  1.0],
        [-1.0,  1.0],
        [-1.0, -1.0],  // cerrado
    ];

    private static ZonaDemo CrearZona() =>
        new(new PoligonoGeo(PoligonoUnitario));

    // ── Contiene ────────────────────────────────────────────────────────────────

    /// <summary>Un punto dentro del polígono devuelve true.</summary>
    [Fact]
    public void Contiene_punto_interior_devuelve_true()
    {
        // Arrange
        var zona = CrearZona();

        // Act / Assert
        Assert.True(zona.Contiene(0.0, 0.0));
    }

    /// <summary>Un punto fuera del polígono devuelve false.</summary>
    [Fact]
    public void Contiene_punto_exterior_devuelve_false()
    {
        // Arrange
        var zona = CrearZona();

        // Act / Assert
        Assert.False(zona.Contiene(2.0, 2.0));
    }

    // ── Clasificar ──────────────────────────────────────────────────────────────

    /// <summary>Celda con las 4 esquinas dentro del polígono se clasifica como Dentro.</summary>
    [Fact]
    public void Clasificar_celda_completamente_interior_es_Dentro()
    {
        // Arrange
        var zona = CrearZona();
        var rect = (-0.4, -0.4, -0.1, -0.1); // (MinLat, MinLon, MaxLat, MaxLon) — completamente adentro

        // Act
        var clase = zona.Clasificar(rect);

        // Assert
        Assert.Equal(ClaseCelda.Dentro, clase);
    }

    /// <summary>Celda que solapa el borde del polígono se clasifica como Borde.</summary>
    [Fact]
    public void Clasificar_celda_en_borde_es_Borde()
    {
        // Arrange
        var zona = CrearZona();
        var rect = (-0.5, 0.5, 0.5, 1.5); // (MinLat, MinLon, MaxLat, MaxLon) — cruza el borde derecho

        // Act
        var clase = zona.Clasificar(rect);

        // Assert
        Assert.Equal(ClaseCelda.Borde, clase);
    }

    /// <summary>Celda completamente fuera del polígono se clasifica como Fuera.</summary>
    [Fact]
    public void Clasificar_celda_completamente_exterior_es_Fuera()
    {
        // Arrange
        var zona = CrearZona();
        var rect = (-3.0, -3.0, -2.0, -2.0); // (MinLat, MinLon, MaxLat, MaxLon) — afuera

        // Act
        var clase = zona.Clasificar(rect);

        // Assert
        Assert.Equal(ClaseCelda.Fuera, clase);
    }

    /// <summary>Celda con una esquina exactamente adentro y el resto afuera se clasifica como Borde.</summary>
    [Fact]
    public void Clasificar_celda_con_una_esquina_adentro_es_Borde()
    {
        // Arrange
        var zona = CrearZona();
        // MinLat=0.5, MinLon=0.5, MaxLat=1.5, MaxLon=1.5 → esquina (0.5,0.5) adentro; (1.5,1.5) afuera
        var rect = (0.5, 0.5, 1.5, 1.5);

        // Act
        var clase = zona.Clasificar(rect);

        // Assert
        Assert.Equal(ClaseCelda.Borde, clase);
    }
}
