using KillaUru.Auditoria.Domain;

namespace KillaUru.Auditoria.Tests.Domain;

/// <summary>
/// Los 14 códigos del catálogo son permanentes: renumerarlos rompe el historial ya persistido.
/// Solo tres acciones generan alerta de riesgo: cambio de rol (2), bloqueo (7) y exportación (13).
/// Spec 002 · FR-005 · CHK002 · T003, T007.
/// </summary>
public sealed class CatalogoAccionesTests
{
    /// <summary>
    /// Cada código del 1 al 14 tiene categoría y los fuera de rango devuelven null;
    /// exactamente los códigos 2, 7 y 13 son de riesgo — ni uno más, ni uno menos.
    /// </summary>
    [Fact]
    public void Todas_las_acciones_tienen_categoria_y_solo_tres_son_de_riesgo()
    {
        // Arrange
        var codigos = Enumerable.Range(1, 14).Select(c => (short)c).ToList();

        // Act
        var categorias = codigos.Select(CatalogoAcciones.Categoria).ToList();
        var deRiesgo = codigos.Where(CatalogoAcciones.EsDeRiesgo).ToArray();

        // Assert — catálogo completo y sin huecos; códigos fuera de rango devuelven null
        Assert.All(categorias, Assert.NotNull);
        Assert.Equal(new short[] { 2, 7, 13 }, deRiesgo);
        Assert.Null(CatalogoAcciones.Categoria(0));
        Assert.Null(CatalogoAcciones.Categoria(15));
    }
}
