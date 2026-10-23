using KillaUru.Auditoria.Domain;
namespace KillaUru.Auditoria.Tests.Domain;

/// <summary>Los códigos persistidos no se renumeran y solo tres acciones generan alertas.</summary>
public sealed class CatalogoAccionesTests
{
    [Fact]
    public void Todas_las_acciones_tienen_categoria_y_solo_tres_son_de_riesgo()
    {
        Assert.All(Enumerable.Range(1, 14), codigo => Assert.NotNull(CatalogoAcciones.Categoria((short)codigo)));
        Assert.Equal(new short[] { 2, 7, 13 }, Enumerable.Range(1, 14).Select(x => (short)x).Where(CatalogoAcciones.EsDeRiesgo));
        Assert.Null(CatalogoAcciones.Categoria(0));
        Assert.Null(CatalogoAcciones.Categoria(15));
    }
}
