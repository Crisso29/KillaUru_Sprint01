using System.Net;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Infrastructure.Correo;
using KillaUru.Identidad.Infrastructure.Seguridad;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace KillaUru.Identidad.Tests.Infrastructure;

/// <summary>
/// Adaptadores de servicios externos: el anti-bot del panel (Cloudflare Turnstile) y el proveedor
/// de correo, aislados detrás de sus puertos. Spec 003 · FR-012, FR-025.
/// </summary>
public sealed class ServiciosExternosTests
{
    private static HttpClient ClienteQueResponde(string cuerpo) =>
        new(new RespuestaHttpPrueba(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(cuerpo) }));

    private static VerificadorTurnstile Turnstile(HttpClient cliente) =>
        new(cliente, Options.Create(new IdentidadOptions { TurnstileSecreto = "ficticio" }));

    /// <summary>Responde 503 al primer envío y 200 después, anotando la clave de idempotencia de cada intento.</summary>
    private static HttpClient ProveedorQueFallaLaPrimeraVez(List<string> clavesEnviadas) =>
        new(new RespuestaHttpPrueba(peticion =>
        {
            clavesEnviadas.Add(peticion.Headers.GetValues("Idempotency-Key").Single());
            var primerIntento = clavesEnviadas.Count == 1;
            return new HttpResponseMessage(primerIntento ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK);
        }));

    /// <summary>Solo un «success: true» de Cloudflare deja pasar el inicio del panel (FR-012).</summary>
    [Theory]
    [InlineData("{\"success\":true}", true)]
    [InlineData("{\"success\":false}", false)]
    public async Task Turnstile_acepta_solo_success_verdadero(string respuestaCloudflare, bool esperado)
    {
        // Arrange
        using var cliente = ClienteQueResponde(respuestaCloudflare);

        // Act
        var humano = await Turnstile(cliente).VerificarAsync("token-sintetico", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(esperado, humano);
    }

    /// <summary>Sin token anti-bot se rechaza sin consultar a Cloudflare (FR-012).</summary>
    [Fact]
    public async Task Turnstile_sin_token_se_rechaza()
    {
        // Arrange
        using var cliente = ClienteQueResponde("{\"success\":true}");

        // Act
        var humano = await Turnstile(cliente).VerificarAsync(null, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(humano);
    }

    /// <summary>
    /// Si el proveedor falla y se reintenta, se usa la misma clave de idempotencia: la persona
    /// no recibe dos correos iguales (FR-025).
    /// </summary>
    [Fact]
    public async Task Correo_reintenta_con_la_misma_clave_de_idempotencia()
    {
        // Arrange
        var clavesEnviadas = new List<string>();
        using var cliente = ProveedorQueFallaLaPrimeraVez(clavesEnviadas);
        var correo = new EnviadorCorreoHttp(cliente, Options.Create(new IdentidadOptions { CorreoApiKey = "ficticia" }),
            TimeProvider.System, NullLogger<EnviadorCorreoHttp>.Instance);

        // Act
        await correo.VerificacionAsync("sintetico@example.com", "https://panel.killauru.pe/cuenta/verificar#token=ficticio",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, clavesEnviadas.Count);
        Assert.Equal(clavesEnviadas[0], clavesEnviadas[1]);
    }
}
