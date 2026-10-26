using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;
namespace KillaUru.Integracion.Tests.Identidad;

public sealed class CuentaApiTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Verificacion_concurrente_solo_permite_un_canje()
    {
        using var e = new EscenarioIdentidad(bd);
        var correo = Guid.NewGuid() + "@example.com";
        await e.RegistrarAsync(correo);
        var token = e.Correo.Token(correo);
        var resultados = await Task.WhenAll(Enumerable.Range(0, 2).Select(_ =>
            e.Cliente.PostAsJsonAsync("/api/v1/auth/verificar-correo", new { token }, TestContext.Current.CancellationToken)));
        Assert.Single(resultados, x => x.StatusCode == HttpStatusCode.NoContent);
        Assert.Single(resultados, x => x.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Registro_verificacion_y_eliminacion_borran_datos_personales()
    {
        using var e = new EscenarioIdentidad(bd);
        var correo = Guid.NewGuid() + "@example.com";
        var sesion = await e.RegistrarAsync(correo);
        Assert.Contains("#token=", e.Correo.Enlaces[correo]);
        e.Autenticar(sesion);
        var datos = await e.Cliente.GetFromJsonAsync<JsonElement>("/api/v1/me", TestContext.Current.CancellationToken);
        Assert.Equal(correo, datos.GetProperty("correo").GetString());
        Assert.False(datos.GetProperty("correoVerificado").GetBoolean());
        Assert.Equal(2, datos.GetProperty("aceptaciones").GetArrayLength());
        var verificacion = await e.Cliente.PostAsJsonAsync("/api/v1/auth/verificar-correo", new { token = e.Correo.Token(correo) }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, verificacion.StatusCode);
        var reuso = await e.Cliente.PostAsJsonAsync("/api/v1/auth/verificar-correo", new { token = e.Correo.Token(correo) }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, reuso.StatusCode);
        using var borrar = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/me") { Content = JsonContent.Create(new { contrasena = EscenarioIdentidad.Contrasena }) };
        Assert.Equal(HttpStatusCode.NoContent, (await e.Cliente.SendAsync(borrar, TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await e.Cliente.GetAsync("/api/v1/me", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Registro_limita_el_sexto_y_acepta_el_quinto()
    {
        using var e = new EscenarioIdentidad(bd);
        for (var i = 0; i < 5; i++) await e.RegistrarAsync(Guid.NewGuid() + "@example.com");
        var sexto = await e.Cliente.PostAsJsonAsync("/api/v1/auth/registro", new
        {
            correo = Guid.NewGuid() + "@example.com", contrasena = EscenarioIdentidad.Contrasena,
            versionPoliticaAceptada = "1.2", versionTerminosAceptada = "1.2",
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.TooManyRequests, sexto.StatusCode);
        Assert.NotNull(sexto.Headers.RetryAfter);
    }

    [Fact]
    public async Task Recuperacion_no_revela_la_cuenta_y_revoca_sesiones()
    {
        using var e = new EscenarioIdentidad(bd);
        var correo = Guid.NewGuid() + "@example.com";
        var sesion = await e.RegistrarAsync(correo);
        var token = e.Correo.Token(correo);
        await e.Cliente.PostAsJsonAsync("/api/v1/auth/verificar-correo", new { token }, TestContext.Current.CancellationToken);
        foreach (var direccion in new[] { correo, "no-existe-" + correo })
        {
            var r = await e.Cliente.PostAsJsonAsync("/api/v1/auth/recuperar", new { correo = direccion }, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.Accepted, r.StatusCode);
            Assert.Equal("", await r.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        }
        var fijar = await e.Cliente.PostAsJsonAsync("/api/v1/auth/fijar-contrasena",
            new { token = e.Correo.Token(correo), nuevaContrasena = "otra-clave-segura" }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, fijar.StatusCode);
        var renovar = await e.Cliente.PostAsJsonAsync("/api/v1/auth/renovar",
            new { tokenRenovacion = sesion.GetProperty("tokenRenovacion").GetString() }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, renovar.StatusCode);
    }

    [Theory]
    [InlineData("1.1", "una-clave-larga", "aceptacion_requerida")]
    [InlineData(null, "una-clave-larga", "aceptacion_requerida")]
    [InlineData("1.2", "123456789", "datos_invalidos")]
    public async Task Registro_rechaza_datos_y_versiones_invalidas(string? version, string clave, string codigo)
    {
        using var e = new EscenarioIdentidad(bd);
        var r = await e.Cliente.PostAsJsonAsync("/api/v1/auth/registro", new
        {
            correo = Guid.NewGuid() + "@example.com", contrasena = clave, versionPoliticaAceptada = version, versionTerminosAceptada = "1.2",
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal(codigo, (await r.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("codigo").GetString());
    }
}
