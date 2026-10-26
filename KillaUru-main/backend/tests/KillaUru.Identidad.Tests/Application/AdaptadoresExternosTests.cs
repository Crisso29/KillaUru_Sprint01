using System.Net;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Infrastructure.Correo;
using KillaUru.Identidad.Infrastructure.Seguridad;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
namespace KillaUru.Identidad.Tests.Application;

public sealed class AdaptadoresExternosTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Turnstile_solo_acepta_success_verdadero(bool exito)
    {
        using var handler = new RespuestaHttpPrueba(_ => new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(exito ? "{\"success\":true}" : "{\"success\":false}") });
        using var cliente = new HttpClient(handler);
        var verificador = new VerificadorTurnstile(cliente, Options.Create(new IdentidadOptions { TurnstileSecreto = "ficticio" }));
        Assert.Equal(exito, await verificador.VerificarAsync("token-sintetico", TestContext.Current.CancellationToken));
        Assert.False(await verificador.VerificarAsync(null, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Correo_reintenta_con_la_misma_clave_de_idempotencia()
    {
        var claves = new List<string>();
        using var handler = new RespuestaHttpPrueba(peticion =>
        {
            claves.Add(peticion.Headers.GetValues("Idempotency-Key").Single());
            return new HttpResponseMessage(claves.Count == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK);
        });
        using var cliente = new HttpClient(handler);
        var correo = new EnviadorCorreoHttp(cliente, Options.Create(new IdentidadOptions { CorreoApiKey = "ficticia" }),
            TimeProvider.System, NullLogger<EnviadorCorreoHttp>.Instance);
        await correo.VerificacionAsync("sintetico@example.com", "https://panel.killauru.pe/cuenta/verificar#token=ficticio", TestContext.Current.CancellationToken);
        Assert.Equal(2, claves.Count);
        Assert.Equal(claves[0], claves[1]);
    }
}
