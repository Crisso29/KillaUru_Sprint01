using KillaUru.Mapa.Domain;

namespace KillaUru.Mapa.Tests.Domain;

/// <summary>
/// Bbox parsea "minLon,minLat,maxLon,maxLat", intersecta celdas y verifica si contiene un punto.
/// Spec 006 · R-003.
/// </summary>
public sealed class BboxTests
{
    // ── TryParse ────────────────────────────────────────────────────────────────

    /// <summary>null y cadena vacía se consideran «sin bbox» → true con resultado null.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryParse_entrada_nula_o_vacia_es_valida_sin_bbox(string? entrada)
    {
        // Arrange / Act
        var ok = Bbox.TryParse(entrada, out var bbox);

        // Assert
        Assert.True(ok);
        Assert.Null(bbox);
    }

    /// <summary>Cadena con 4 valores válidos devuelve true y un Bbox con los valores correctos.</summary>
    [Fact]
    public void TryParse_cadena_valida_devuelve_bbox()
    {
        // Arrange
        const string entrada = "-74.33,-13.25,-74.22,-13.16";

        // Act
        var ok = Bbox.TryParse(entrada, out var bbox);

        // Assert
        Assert.True(ok);
        Assert.NotNull(bbox);
        Assert.Equal(-74.33, bbox!.MinLon, 6);
        Assert.Equal(-13.25, bbox.MinLat, 6);
        Assert.Equal(-74.22, bbox.MaxLon, 6);
        Assert.Equal(-13.16, bbox.MaxLat, 6);
    }

    /// <summary>Formato incorrecto (3 partes, no numérico, invertido) devuelve false.</summary>
    [Theory]
    [InlineData("1,2,3")]
    [InlineData("a,b,c,d")]
    [InlineData("10,0,-10,1")]   // minLon > maxLon
    [InlineData("0,10,1,-10")]   // minLat > maxLat
    [InlineData("200,0,201,1")]  // lon fuera de rango
    [InlineData("0,100,1,101")]  // lat fuera de rango
    public void TryParse_formato_incorrecto_devuelve_false(string entrada)
    {
        // Arrange / Act
        var ok = Bbox.TryParse(entrada, out _);

        // Assert
        Assert.False(ok);
    }

    // ── Intersecta ──────────────────────────────────────────────────────────────

    /// <summary>Bbox que se superpone con un rectángulo devuelve true.</summary>
    [Fact]
    public void Intersecta_rectangulo_solapado_devuelve_true()
    {
        // Arrange
        var bbox = new Bbox(-74.33, -13.25, -74.22, -13.16);
        var rect = (-13.24, -74.30, -13.17, -74.25); // (MinLat, MinLon, MaxLat, MaxLon)

        // Act
        var resultado = bbox.Intersecta(rect);

        // Assert
        Assert.True(resultado);
    }

    /// <summary>Bbox completamente separado de un rectángulo devuelve false.</summary>
    [Fact]
    public void Intersecta_rectangulo_separado_devuelve_false()
    {
        // Arrange
        var bbox = new Bbox(-74.33, -13.25, -74.22, -13.16);
        var rect = (-13.00, -74.00, -12.90, -73.90);

        // Act
        var resultado = bbox.Intersecta(rect);

        // Assert
        Assert.False(resultado);
    }

    // ── Contiene ────────────────────────────────────────────────────────────────

    /// <summary>Un punto dentro del bbox devuelve true.</summary>
    [Fact]
    public void Contiene_punto_dentro_devuelve_true()
    {
        // Arrange
        var bbox = new Bbox(-74.33, -13.25, -74.22, -13.16);

        // Act / Assert
        Assert.True(bbox.Contiene(-13.20, -74.28));
    }

    /// <summary>Un punto fuera del bbox devuelve false.</summary>
    [Fact]
    public void Contiene_punto_fuera_devuelve_false()
    {
        // Arrange
        var bbox = new Bbox(-74.33, -13.25, -74.22, -13.16);

        // Act / Assert
        Assert.False(bbox.Contiene(-13.00, -74.00));
    }
}
