using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;
namespace KillaUru.Integracion.Tests.Identidad;

public sealed class SesionApiTests(BaseDatosPrueba bd)
{
    [Theory]
    [InlineData(59, HttpStatusCode.OK)]
    [InlineData(60, HttpStatusCode.OK)]
    [InlineData(61, HttpStatusCode.Unauthorized)]
    public async Task Renovacion_gracia_y_reuso_revocan_la_familia(int segundos, HttpStatusCode esperado)
    {
        using var e = new EscenarioIdentidad(bd);
        var sesion = await e.RegistrarAsync(Guid.NewGuid() + "@example.com");
        var cuerpo = new { tokenRenovacion = sesion.GetProperty("tokenRenovacion").GetString() };
        var primera = await e.Cliente.PostAsJsonAsync("/api/v1/auth/renovar", cuerpo, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, primera.StatusCode);
        var nuevo = (await primera.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("tokenRenovacion").GetString();
        e.Reloj.Advance(TimeSpan.FromSeconds(segundos));
        var segunda = await e.Cliente.PostAsJsonAsync("/api/v1/auth/renovar", cuerpo, TestContext.Current.CancellationToken);
        Assert.Equal(esperado, segunda.StatusCode);
        if (esperado == HttpStatusCode.OK)
        {
            Assert.Equal(nuevo, (await segunda.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("tokenRenovacion").GetString());
        }
        else
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await e.Cliente.PostAsJsonAsync("/api/v1/auth/renovar",
                new { tokenRenovacion = nuevo }, TestContext.Current.CancellationToken)).StatusCode);
        }
    }

    [Fact]
    public async Task Renovaciones_simultaneas_devuelven_el_mismo_sucesor()
    {
        using var e = new EscenarioIdentidad(bd);
        var sesion = await e.RegistrarAsync(Guid.NewGuid() + "@example.com");
        var cuerpo = new { tokenRenovacion = sesion.GetProperty("tokenRenovacion").GetString() };
        var respuestas = await Task.WhenAll(Enumerable.Range(0, 2).Select(_ =>
            e.Cliente.PostAsJsonAsync("/api/v1/auth/renovar", cuerpo, TestContext.Current.CancellationToken)));
        var tokens = new List<string?>();
        foreach (var respuesta in respuestas)
        {
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            tokens.Add((await respuesta.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("tokenRenovacion").GetString());
        }
        Assert.Equal(tokens[0], tokens[1]);
    }

    [Fact]
    public async Task Jwt_aceptado_a_459_y_rechazado_a_501()
    {
        using var e = new EscenarioIdentidad(bd);
        e.Autenticar(await e.RegistrarAsync(Guid.NewGuid() + "@example.com"));
        e.Reloj.Advance(TimeSpan.FromMinutes(4) + TimeSpan.FromSeconds(59));
        Assert.Equal(HttpStatusCode.OK, (await e.Cliente.GetAsync("/api/v1/me", TestContext.Current.CancellationToken)).StatusCode);
        e.Reloj.Advance(TimeSpan.FromSeconds(2));
        Assert.Equal(HttpStatusCode.Unauthorized, (await e.Cliente.GetAsync("/api/v1/me", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Inicio_de_sesion_bloquea_y_no_revela_correos()
    {
        using var e = new EscenarioIdentidad(bd);
        var correo = Guid.NewGuid() + "@example.com";
        await e.RegistrarAsync(correo);
        foreach (var direccion in new[] { "inexistente-" + correo, correo, correo, correo })
        {
            var r = await e.Cliente.PostAsJsonAsync("/api/v1/auth/inicio-sesion",
                new { correo = direccion, contrasena = "incorrecta", canal = "app" }, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
            Assert.Equal("credenciales_invalidas", (await r.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("codigo").GetString());
        }
        var bloqueada = await e.Cliente.PostAsJsonAsync("/api/v1/auth/inicio-sesion",
            new { correo, contrasena = EscenarioIdentidad.Contrasena, canal = "app" }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, bloqueada.StatusCode);
        e.Reloj.Advance(TimeSpan.FromMinutes(5));
        var valida = await e.Cliente.PostAsJsonAsync("/api/v1/auth/inicio-sesion",
            new { correo, contrasena = EscenarioIdentidad.Contrasena, canal = "app" }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, valida.StatusCode);
    }

    [Fact]
    public async Task Cierre_revoca_refresh_y_conductor_no_entra_al_panel()
    {
        using var e = new EscenarioIdentidad(bd);
        var correo = Guid.NewGuid() + "@example.com";
        var sesion = await e.RegistrarAsync(correo);
        var panel = await e.Cliente.PostAsJsonAsync("/api/v1/auth/inicio-sesion", new
        {
            correo, contrasena = EscenarioIdentidad.Contrasena, canal = "panel", turnstileToken = "antibot-prueba",
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, panel.StatusCode);
        e.Autenticar(sesion);
        Assert.Equal(HttpStatusCode.NoContent, (await e.Cliente.PostAsync("/api/v1/auth/cerrar-sesion", null, TestContext.Current.CancellationToken)).StatusCode);
        e.Cliente.DefaultRequestHeaders.Authorization = null;
        Assert.Equal(HttpStatusCode.Unauthorized, (await e.Cliente.PostAsJsonAsync("/api/v1/auth/renovar",
            new { tokenRenovacion = sesion.GetProperty("tokenRenovacion").GetString() }, TestContext.Current.CancellationToken)).StatusCode);
    }
}
