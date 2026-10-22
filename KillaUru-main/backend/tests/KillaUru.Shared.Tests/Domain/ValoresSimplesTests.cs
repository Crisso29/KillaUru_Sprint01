using KillaUru.Shared.Domain;

namespace KillaUru.Shared.Tests.Domain;

public sealed class CoordenadaTests
{
    [Theory]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    [InlineData(-13.2, -74.2)]
    public void Crear_dentro_de_rango_es_valida(double lat, double lon) => Assert.NotNull(Coordenada.Crear(lat, lon));

    [Theory]
    [InlineData(-90.0001, 0)]
    [InlineData(90.0001, 0)]
    [InlineData(0, 180.0001)]
    [InlineData(0, -180.0001)]
    [InlineData(double.NaN, 0)]
    [InlineData(0, double.PositiveInfinity)]
    public void Crear_fuera_de_rango_es_null(double lat, double lon) => Assert.Null(Coordenada.Crear(lat, lon));
}

public sealed class ConfidenceTests
{
    [Theory]
    [InlineData(0.0)]
    [InlineData(0.75)]
    [InlineData(1.0)]
    public void Crear_entre_0_y_1_es_valida(double valor) => Assert.Equal(valor, Confidence.Crear(valor)!.Value.Valor);

    [Theory]
    [InlineData(-0.0001)]
    [InlineData(1.0001)]
    [InlineData(double.NaN)]
    public void Crear_fuera_de_0_a_1_es_null(double valor) => Assert.Null(Confidence.Crear(valor));
}

public sealed class SeudonimoInstalacionTests
{
    [Fact]
    public void Crear_con_un_uuid_es_valido()
    {
        var texto = "6b1c9d3e-2f4a-4b8e-9c7d-1a2b3c4d5e6f";

        Assert.Equal(Guid.Parse(texto), SeudonimoInstalacion.Crear(texto)!.Value.Valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("no-es-un-uuid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Crear_sin_un_uuid_util_es_null(string? texto) => Assert.Null(SeudonimoInstalacion.Crear(texto));
}
