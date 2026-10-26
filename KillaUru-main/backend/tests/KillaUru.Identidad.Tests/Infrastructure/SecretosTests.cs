using KillaUru.Identidad.Application;
using KillaUru.Identidad.Infrastructure.Seguridad;
using Microsoft.Extensions.Options;

namespace KillaUru.Identidad.Tests.Infrastructure;

/// <summary>
/// Secretos de sesión y contraseñas: tokens aleatorios de 32 bytes guardados solo como SHA-256
/// y contraseñas con PBKDF2 con sal. Spec 003 · FR-005, FR-015, FR-016.
/// </summary>
public sealed class SecretosTests
{
    private static GeneradorSecretos Generador(char clave) =>
        new(Options.Create(new IdentidadOptions { ClaveRotacion = new string(clave, 32) }));

    /// <summary>32 bytes aleatorios en Base64Url son 43 caracteres; dos tokens nunca se repiten (FR-015).</summary>
    [Fact]
    public void Token_es_aleatorio_de_32_bytes()
    {
        // Arrange
        var generador = Generador('a');

        // Act
        var primero = generador.Aleatorio32();
        var segundo = generador.Aleatorio32();

        // Assert
        Assert.Equal(43, primero.Length);
        Assert.NotEqual(primero, segundo);
    }

    /// <summary>En la base solo se guarda el SHA-256 (32 bytes), nunca el token (FR-015).</summary>
    [Fact]
    public void Hash_es_sha256()
    {
        // Act
        var hash = Generador('a').Hash("token");

        // Assert
        Assert.Equal(32, hash.Length);
    }

    /// <summary>
    /// El sucesor se deriva del token anterior con la clave del servidor: es determinista para
    /// devolver el mismo par en un reintento (FR-016), pero sin la clave no se puede adivinar.
    /// </summary>
    [Fact]
    public void Derivacion_es_determinista_y_depende_de_la_clave()
    {
        // Arrange
        var token = Generador('a').Aleatorio32();

        // Act
        var conClaveA = Generador('a').Derivar(token);
        var conClaveARepetida = Generador('a').Derivar(token);
        var conClaveB = Generador('b').Derivar(token);

        // Assert
        Assert.Equal(conClaveA, conClaveARepetida);
        Assert.NotEqual(conClaveA, conClaveB);
    }

    /// <summary>Con sal, la misma contraseña produce hashes distintos (FR-005).</summary>
    [Fact]
    public void Contrasena_se_guarda_con_sal()
    {
        // Arrange
        var hasher = new HashContrasenas();

        // Act
        var primero = hasher.Hash("una-clave-larga");
        var segundo = hasher.Hash("una-clave-larga");

        // Assert
        Assert.NotEqual(primero, segundo);
    }

    /// <summary>Verifica la correcta y rechaza la incorrecta o una cuenta sin contraseña (invitación).</summary>
    [Theory]
    [InlineData("una-clave-larga", true, true)]
    [InlineData("incorrecta", true, false)]
    [InlineData("una-clave-larga", false, false)]
    public void Verificar_contrasena(string intento, bool tieneHash, bool esperado)
    {
        // Arrange
        var hasher = new HashContrasenas();
        var guardado = tieneHash ? hasher.Hash("una-clave-larga") : null;

        // Act
        var correcta = hasher.Verificar(guardado, intento, out var requiereRehash);

        // Assert
        Assert.Equal(esperado, correcta);
        Assert.False(requiereRehash);
    }
}
