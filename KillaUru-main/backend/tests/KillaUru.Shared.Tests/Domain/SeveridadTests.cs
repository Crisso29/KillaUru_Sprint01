using KillaUru.Shared.Domain;

namespace KillaUru.Shared.Tests.Domain;

public sealed class SeveridadTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Crear_fuera_de_1_a_5_es_null(int valor) => Assert.Null(Severidad.Crear(valor));

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Crear_en_los_limites_es_valida(int valor) => Assert.Equal(valor, Severidad.Crear(valor)!.Value.Valor);

    [Theory]
    [InlineData(new[] { 2, 3, 3, 5 }, 3)]   // RN-05: impar en el medio → 3
    [InlineData(new[] { 3, 4 }, 4)]         // par: promedio 3,5 → redondea hacia arriba
    [InlineData(new[] { 5 }, 5)]
    [InlineData(new[] { 1, 2 }, 2)]
    [InlineData(new[] { 4, 1, 5 }, 4)]      // no depende del orden de llegada
    public void MedianaHaciaArriba_sigue_RN05(int[] valores, int esperada)
    {
        var severidades = valores.Select(v => Severidad.Crear(v)!.Value);

        Assert.Equal(esperada, Severidad.MedianaHaciaArriba(severidades).Valor);
    }

    [Fact]
    public void MedianaHaciaArriba_sin_valores_no_tiene_sentido() =>
        Assert.Throws<ArgumentException>(() => Severidad.MedianaHaciaArriba([]));

    [Theory]
    [InlineData(3, false)]
    [InlineData(4, true)]
    [InlineData(5, true)]
    public void EsAlta_desde_4(int valor, bool alta) => Assert.Equal(alta, Severidad.Crear(valor)!.Value.EsAlta);
}
