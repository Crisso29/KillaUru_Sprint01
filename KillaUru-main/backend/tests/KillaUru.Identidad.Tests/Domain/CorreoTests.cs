using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Tests.Domain;

/// <summary>
/// El correo es la identidad de la cuenta: se normaliza para que la unicidad no dependa de
/// mayúsculas ni espacios, y un formato inválido se rechaza sin excepciones.
/// Spec 003 · FR-001, FR-006.
/// </summary>
public sealed class CorreoTests
{
    /// <summary>«A@x.com» y «a@x.com» deben ser la misma cuenta (FR-006, citext UNIQUE).</summary>
    [Fact]
    public void Normaliza_espacios_y_mayusculas()
    {
        // Arrange
        const string escrito = "  CONDUCTOR@Example.COM ";

        // Act
        var correo = Correo.Crear(escrito);

        // Assert
        Assert.Equal("conductor@example.com", correo?.Valor);
    }

    /// <summary>Un correo mal formado no puede recibir el enlace de verificación (FR-021).</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("sin-arroba")]
    [InlineData("@example.com")]
    [InlineData("a@dominio")]
    [InlineData("a@.example.com")]
    [InlineData("a b@example.com")]
    [InlineData("a@@example.com")]
    public void Rechaza_formato_invalido(string? escrito)
    {
        // Act
        var correo = Correo.Crear(escrito);

        // Assert
        Assert.Null(correo);
    }

    /// <summary>254 caracteres es el máximo de RFC 5321; uno más se rechaza.</summary>
    [Theory]
    [InlineData(242, true)]
    [InlineData(243, false)]
    public void Respeta_el_limite_de_254_caracteres(int largoUsuario, bool esValido)
    {
        // Arrange
        var escrito = new string('a', largoUsuario) + "@example.com";

        // Act
        var correo = Correo.Crear(escrito);

        // Assert
        Assert.Equal(esValido, correo is not null);
    }
}
