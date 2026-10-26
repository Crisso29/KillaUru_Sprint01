using System.Net;
using System.Net.Http.Json;
using KillaUru.Integracion.Tests.Infraestructura;
namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>La autorización precede a la validación del cuerpo en cada ruta administrativa.</summary>
public sealed class RespuestasComunesApiTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Todas_las_operaciones_administrativas_exigen_token_y_rol()
    {
        using var e = new EscenarioIdentidad(bd);
        var id = Guid.NewGuid();
        var rutas = new (HttpMethod Metodo, string Ruta)[]
        {
            (HttpMethod.Get, "/api/v1/usuarios"), (HttpMethod.Post, "/api/v1/usuarios"),
            (HttpMethod.Put, $"/api/v1/usuarios/{id}/rol"), (HttpMethod.Post, $"/api/v1/usuarios/{id}/desactivar"),
            (HttpMethod.Post, $"/api/v1/usuarios/{id}/activar"), (HttpMethod.Post, $"/api/v1/usuarios/{id}/reenviar-correo"),
            (HttpMethod.Get, "/api/v1/usuarios/revision-privilegios"), (HttpMethod.Post, "/api/v1/usuarios/revision-privilegios"),
        };
        foreach (var estado in new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden })
        {
            foreach (var (metodo, ruta) in rutas)
            {
                using var peticion = new HttpRequestMessage(metodo, ruta) { Content = JsonContent.Create(new { }) };
                Assert.Equal(estado, (await e.Cliente.SendAsync(peticion, TestContext.Current.CancellationToken)).StatusCode);
            }
            if (estado == HttpStatusCode.Unauthorized) e.Autenticar(await e.RegistrarAsync(Guid.NewGuid() + "@example.com"));
        }
    }

    [Fact]
    public async Task Panel_exige_antibot_y_expone_bloqueo_con_retry_after()
    {
        using var e = new EscenarioIdentidad(bd);
        var correo = await AdministradoresPrueba.CrearAsync(e);
        var sinAntiBot = await e.Cliente.PostAsJsonAsync("/api/v1/auth/inicio-sesion",
            new { correo, contrasena = EscenarioIdentidad.Contrasena, canal = "panel" }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, sinAntiBot.StatusCode);
        for (var i = 0; i < 3; i++)
        {
            await e.Cliente.PostAsJsonAsync("/api/v1/auth/inicio-sesion",
                new { correo, contrasena = "incorrecta", canal = "panel", turnstileToken = "antibot-prueba" }, TestContext.Current.CancellationToken);
        }
        var bloqueada = await e.Cliente.PostAsJsonAsync("/api/v1/auth/inicio-sesion",
            new { correo, contrasena = EscenarioIdentidad.Contrasena, canal = "panel", turnstileToken = "antibot-prueba" }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Locked, bloqueada.StatusCode);
        Assert.NotNull(bloqueada.Headers.RetryAfter);
        Assert.Contains("bloqueadoHastaUtc", await bloqueada.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }
}
