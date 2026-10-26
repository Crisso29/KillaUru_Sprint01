using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>
/// Ciclo de la cuenta del conductor por la API real: registro, verificación del correo,
/// mis datos y recuperación de contraseña.
/// Spec 003 · FR-001, FR-002, FR-005, FR-021, FR-022, FR-023, FR-024, FR-026, FR-037.
/// </summary>
public sealed class CuentaApiTests(BaseDatosPrueba bd)
{
    /// <summary>Versión vieja, sin aceptar o contraseña de 9 caracteres: 400 con su código (FR-002, FR-005).</summary>
    [Theory]
    [InlineData("1.1", EscenarioIdentidad.Contrasena, "aceptacion_requerida")]
    [InlineData(null, EscenarioIdentidad.Contrasena, "aceptacion_requerida")]
    [InlineData("1.2", "123456789", "datos_invalidos")]
    public async Task Registro_rechaza_datos_y_versiones_invalidas(string? versionPolitica, string contrasena, string codigoEsperado)
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);

        // Act
        var respuesta = await escenario.RegistroAsync(EscenarioIdentidad.CorreoNuevo(), contrasena, versionPolitica);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal(codigoEsperado, await EscenarioIdentidad.CodigoAsync(respuesta));
    }

    /// <summary>5 registros por hora por IP; el sexto recibe 429 con Retry-After (FR-037).</summary>
    [Fact]
    public async Task Sexto_registro_desde_la_misma_ip_se_limita()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        for (var registro = 0; registro < 5; registro++) await escenario.RegistrarAsync(EscenarioIdentidad.CorreoNuevo());

        // Act
        var sexto = await escenario.RegistroAsync(EscenarioIdentidad.CorreoNuevo());

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, sexto.StatusCode);
        Assert.NotNull(sexto.Headers.RetryAfter);
    }

    /// <summary>
    /// Mis datos muestra correo, estado de verificación y las dos aceptaciones (FR-026); el enlace
    /// de verificación lleva el token en el fragmento para que no quede en logs (FR-024).
    /// </summary>
    [Fact]
    public async Task Registro_envia_verificacion_y_muestra_mis_datos()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var correo = EscenarioIdentidad.CorreoNuevo();
        escenario.Autenticar(await escenario.RegistrarAsync(correo));

        // Act
        var datos = await escenario.Cliente.GetFromJsonAsync<JsonElement>("/api/v1/me", EscenarioIdentidad.Ct);

        // Assert
        Assert.Contains("#token=", escenario.Correo.Enlaces[correo]);
        Assert.Equal(correo, datos.GetProperty("correo").GetString());
        Assert.False(datos.GetProperty("correoVerificado").GetBoolean());
        Assert.Equal(2, datos.GetProperty("aceptaciones").GetArrayLength());
    }

    /// <summary>El enlace de verificación sirve una sola vez (FR-021).</summary>
    [Fact]
    public async Task Enlace_de_verificacion_es_de_un_solo_uso()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var correo = EscenarioIdentidad.CorreoNuevo();
        await escenario.RegistrarAsync(correo);
        var token = escenario.Correo.Token(correo);

        // Act
        var primerUso = await escenario.VerificarCorreoAsync(token);
        var segundoUso = await escenario.VerificarCorreoAsync(token);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, primerUso.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, segundoUso.StatusCode);
    }

    /// <summary>Dos canjes simultáneos del mismo enlace: el bloqueo de fila deja pasar solo uno (FR-021).</summary>
    [Fact]
    public async Task Verificacion_concurrente_solo_permite_un_canje()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var correo = EscenarioIdentidad.CorreoNuevo();
        await escenario.RegistrarAsync(correo);
        var token = escenario.Correo.Token(correo);

        // Act
        var resultados = await Task.WhenAll(escenario.VerificarCorreoAsync(token), escenario.VerificarCorreoAsync(token));

        // Assert
        Assert.Single(resultados, r => r.StatusCode == HttpStatusCode.NoContent);
        Assert.Single(resultados, r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// La respuesta es 202 vacía exista o no la cuenta: nadie puede averiguar qué correos están
    /// registrados (FR-022).
    /// </summary>
    [Fact]
    public async Task Recuperacion_responde_igual_exista_o_no_la_cuenta()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var correo = EscenarioIdentidad.CorreoNuevo();
        await escenario.RegistrarAsync(correo);
        await escenario.VerificarCorreoAsync(escenario.Correo.Token(correo));

        // Act
        var existente = await escenario.Cliente.PostAsJsonAsync("/api/v1/auth/recuperar", new { correo }, EscenarioIdentidad.Ct);
        var inexistente = await escenario.Cliente.PostAsJsonAsync("/api/v1/auth/recuperar", new { correo = "no-existe-" + correo }, EscenarioIdentidad.Ct);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, existente.StatusCode);
        Assert.Equal(HttpStatusCode.Accepted, inexistente.StatusCode);
        Assert.Equal("", await existente.Content.ReadAsStringAsync(EscenarioIdentidad.Ct));
        Assert.Equal("", await inexistente.Content.ReadAsStringAsync(EscenarioIdentidad.Ct));
    }

    /// <summary>Restablecer la contraseña cierra todas las sesiones abiertas (FR-023).</summary>
    [Fact]
    public async Task Restablecer_contrasena_cierra_las_sesiones()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var correo = EscenarioIdentidad.CorreoNuevo();
        var sesion = await escenario.RegistrarAsync(correo);
        await escenario.VerificarCorreoAsync(escenario.Correo.Token(correo));
        await escenario.Cliente.PostAsJsonAsync("/api/v1/auth/recuperar", new { correo }, EscenarioIdentidad.Ct);

        // Act
        var fijar = await escenario.FijarContrasenaAsync(escenario.Correo.Token(correo), "otra-clave-segura");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, fijar.StatusCode);
        var renovar = await escenario.RenovarAsync(EscenarioIdentidad.TokenRenovacion(sesion));
        Assert.Equal(HttpStatusCode.Unauthorized, renovar.StatusCode);
    }
}
