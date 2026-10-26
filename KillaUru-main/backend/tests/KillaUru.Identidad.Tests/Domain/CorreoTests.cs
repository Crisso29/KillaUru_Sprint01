using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Tests.Domain;

public sealed class CorreoTests
{
    [Fact]
    public void NormalizaEspaciosYMayusculas() =>
        Assert.Equal("conductor@example.com", Correo.Crear("  CONDUCTOR@Example.COM ")?.Valor);

    [Theory]
    [InlineData("")]
    [InlineData("sin-arroba")]
    [InlineData("a@dominio")]
    [InlineData("a b@example.com")]
    [InlineData("a@@example.com")]
    public void RechazaFormatoInvalido(string valor) => Assert.Null(Correo.Crear(valor));

    [Fact]
    public void LimiteDe254Caracteres()
    {
        Assert.NotNull(Correo.Crear(new string('a', 242) + "@example.com"));
        Assert.Null(Correo.Crear(new string('a', 243) + "@example.com"));
    }
}
