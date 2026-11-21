using System.Net;
using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>
/// Inicio, renovación y cierre de sesión por la API real con el reloj controlado.
/// Spec 003 · FR-008, FR-009, FR-013, FR-014, FR-015, FR-016, FR-018.
/// </summary>
public sealed class SesionApiTests(BaseDatosPrueba bd)
{
    /// <summary>
    /// Reusar el token ya rotado hasta 60 s después (reintento por red) devuelve el mismo
    /// sucesor, no uno nuevo (FR-016).
    /// </summary>
    [Theory]
    [InlineData(59)]
    [InlineData(60)]
    public async Task Reuso_dentro_de_la_gracia_devuelve_el_mismo_par(int segundosDespues)
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var tokenOriginal = EscenarioIdentidad.TokenRenovacion(await escenario.RegistrarAsync(EscenarioIdentidad.CorreoNuevo()));
        var sucesor = await EscenarioIdentidad.PropiedadAsync(await escenario.RenovarAsync(tokenOriginal), "tokenRenovacion");
        escenario.Reloj.Advance(TimeSpan.FromSeconds(segundosDespues));

        // Act
        var reuso = await escenario.RenovarAsync(tokenOriginal);

        // Assert
        Assert.Equal(HttpStatusCode.OK, reuso.StatusCode);
        Assert.Equal(sucesor, await EscenarioIdentidad.PropiedadAsync(reuso, "tokenRenovacion"));
    }

    /// <summary>Reusar un token rotado pasada la gracia se trata como robo: cae toda la familia (FR-016).</summary>
    [Fact]
    public async Task Reuso_fuera_de_la_gracia_revoca_toda_la_familia()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var tokenOriginal = EscenarioIdentidad.TokenRenovacion(await escenario.RegistrarAsync(EscenarioIdentidad.CorreoNuevo()));
        var sucesor = await EscenarioIdentidad.PropiedadAsync(await escenario.RenovarAsync(tokenOriginal), "tokenRenovacion");
        escenario.Reloj.Advance(TimeSpan.FromSeconds(61));

        // Act
        var reuso = await escenario.RenovarAsync(tokenOriginal);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, reuso.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await escenario.RenovarAsync(sucesor)).StatusCode);
    }

    /// <summary>Dos renovaciones simultáneas (app con dos pestañas o reintentos) reciben el mismo sucesor (FR-016).</summary>
    [Fact]
    public async Task Renovaciones_simultaneas_devuelven_el_mismo_sucesor()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var token = EscenarioIdentidad.TokenRenovacion(await escenario.RegistrarAsync(EscenarioIdentidad.CorreoNuevo()));

        // Act
        var respuestas = await Task.WhenAll(escenario.RenovarAsync(token), escenario.RenovarAsync(token));

        // Assert
        Assert.All(respuestas, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        var sucesores = await Task.WhenAll(respuestas.Select(r => EscenarioIdentidad.PropiedadAsync(r, "tokenRenovacion")));
        Assert.Equal(sucesores[0], sucesores[1]);
    }

    /// <summary>El JWT de acceso dura 5 min sin tolerancia de reloj: vale a 4:59 y no a 5:01 (FR-014).</summary>
    [Theory]
    [InlineData(299, HttpStatusCode.OK)]
    [InlineData(301, HttpStatusCode.Unauthorized)]
    public async Task Jwt_de_acceso_dura_cinco_minutos_exactos(int segundosDespues, HttpStatusCode esperado)
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        escenario.Autenticar(await escenario.RegistrarAsync(EscenarioIdentidad.CorreoNuevo()));
        escenario.Reloj.Advance(TimeSpan.FromSeconds(segundosDespues));

        // Act
        var respuesta = await escenario.MisDatosAsync();

        // Assert
        Assert.Equal(esperado, respuesta.StatusCode);
    }

    /// <summary>Correo inexistente y contraseña incorrecta reciben la misma respuesta (FR-008).</summary>
    [Fact]
    public async Task Inicio_fallido_no_revela_si_el_correo_existe()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var correo = EscenarioIdentidad.CorreoNuevo();
        await escenario.RegistrarAsync(correo);

        // Act
        var inexistente = await escenario.IniciarSesionAsync("inexistente-" + correo, "incorrecta");
        var claveIncorrecta = await escenario.IniciarSesionAsync(correo, "incorrecta");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, inexistente.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, claveIncorrecta.StatusCode);
        Assert.Equal("credenciales_invalidas", await EscenarioIdentidad.CodigoAsync(inexistente));
        Assert.Equal("credenciales_invalidas", await EscenarioIdentidad.CodigoAsync(claveIncorrecta));
    }

    /// <summary>Tras 3 fallos, ni la clave correcta entra hasta que pasan 5 min (FR-009).</summary>
    [Fact]
    public async Task Tercer_fallo_bloquea_cinco_minutos()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var correo = EscenarioIdentidad.CorreoNuevo();
        await escenario.RegistrarAsync(correo);
        for (var fallo = 0; fallo < 3; fallo++) await escenario.IniciarSesionAsync(correo, "incorrecta");

        // Act
        var duranteElBloqueo = await escenario.IniciarSesionAsync(correo);
        escenario.Reloj.Advance(TimeSpan.FromMinutes(5));
        var alTerminar = await escenario.IniciarSesionAsync(correo);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, duranteElBloqueo.StatusCode);
        Assert.Equal(HttpStatusCode.OK, alTerminar.StatusCode);
    }

    /// <summary>Un conductor con credenciales válidas no entra al panel (FR-013).</summary>
    [Fact]
    public async Task Conductor_no_entra_al_panel()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var correo = EscenarioIdentidad.CorreoNuevo();
        await escenario.RegistrarAsync(correo);

        // Act
        var panel = await escenario.IniciarSesionAsync(correo, canal: "panel", turnstileToken: EscenarioIdentidad.AntiBotValido);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, panel.StatusCode);
    }

    /// <summary>Cerrar sesión revoca el token de renovación de esa familia (FR-018).</summary>
    [Fact]
    public async Task Cerrar_sesion_revoca_la_renovacion()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var sesion = await escenario.RegistrarAsync(EscenarioIdentidad.CorreoNuevo());
        escenario.Autenticar(sesion);

        // Act
        var cierre = await escenario.CerrarSesionAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, cierre.StatusCode);
        escenario.QuitarAutenticacion();
        Assert.Equal(HttpStatusCode.Unauthorized, (await escenario.RenovarAsync(EscenarioIdentidad.TokenRenovacion(sesion))).StatusCode);
    }
}
