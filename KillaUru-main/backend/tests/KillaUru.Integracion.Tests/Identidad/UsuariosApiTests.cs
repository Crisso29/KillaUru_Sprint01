using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;
namespace KillaUru.Integracion.Tests.Identidad;

public sealed class UsuariosApiTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Gestion_permisos_busqueda_desactivacion_y_revision()
    {
        using var e = new EscenarioIdentidad(bd);
        Assert.Equal(HttpStatusCode.Unauthorized, (await e.Cliente.GetAsync("/api/v1/usuarios", TestContext.Current.CancellationToken)).StatusCode);
        var conductor = await e.RegistrarAsync(Guid.NewGuid() + "@example.com");
        e.Autenticar(conductor);
        Assert.Equal(HttpStatusCode.Forbidden, (await e.Cliente.GetAsync("/api/v1/usuarios", TestContext.Current.CancellationToken)).StatusCode);
        e.Cliente.DefaultRequestHeaders.Authorization = null;
        var correo = await AdministradoresPrueba.CrearAsync(e);
        await AdministradoresPrueba.AutenticarAppAsync(e, correo);
        var lista = await e.Cliente.GetFromJsonAsync<JsonElement>("/api/v1/usuarios?busqueda=" + correo, TestContext.Current.CancellationToken);
        Assert.Equal(1, lista.GetProperty("total").GetInt32());
        var id = lista.GetProperty("elementos")[0].GetProperty("id").GetGuid();
        var propio = await e.Cliente.PostAsJsonAsync($"/api/v1/usuarios/{id}/desactivar", new { motivo = "prueba" }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, propio.StatusCode);
        var ids = await e.Cliente.GetFromJsonAsync<JsonElement>($"/api/v1/usuarios?ids={id}", TestContext.Current.CancellationToken);
        Assert.Equal(1, ids.GetProperty("total").GetInt32());
        var revision = await e.Cliente.GetFromJsonAsync<JsonElement>("/api/v1/usuarios/revision-privilegios", TestContext.Current.CancellationToken);
        var confirmados = revision.GetProperty("administradores").EnumerateArray().Select(x => x.GetProperty("id").GetGuid()).ToArray();
        var guardar = await e.Cliente.PostAsJsonAsync("/api/v1/usuarios/revision-privilegios",
            new { confirmados, retirados = Array.Empty<Guid>() }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, guardar.StatusCode);
    }

    [Fact]
    public async Task Panel_cookies_seguras_renueva_con_acceso_vencido()
    {
        using var e = new EscenarioIdentidad(bd);
        var correo = await AdministradoresPrueba.CrearAsync(e);
        var login = await e.Cliente.PostAsJsonAsync("/api/v1/auth/inicio-sesion", new
        {
            correo, contrasena = EscenarioIdentidad.Contrasena, canal = "panel", turnstileToken = "antibot-prueba",
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var cuerpo = await login.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.False(cuerpo.TryGetProperty("tokenAcceso", out _));
        var cookies = login.Headers.GetValues("Set-Cookie").ToArray();
        Assert.Equal(2, cookies.Length);
        Assert.All(cookies, x => { Assert.Contains("httponly", x); Assert.Contains("secure", x); Assert.Contains("samesite=strict", x); });
        Assert.Contains(cookies, x => x.StartsWith("ku_renovacion=", StringComparison.Ordinal) && x.Contains("path=/api/v1/auth/renovar", StringComparison.Ordinal));
        e.Reloj.Advance(TimeSpan.FromMinutes(6));
        e.Cliente.DefaultRequestHeaders.Add("Cookie", string.Join("; ", cookies.Select(x => x.Split(';')[0])));
        var renovar = await e.Cliente.PostAsync("/api/v1/auth/renovar", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, renovar.StatusCode);
        Assert.False((await renovar.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).TryGetProperty("tokenAcceso", out _));
    }
}
