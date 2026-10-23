using System.Net;
using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Seguridad;

/// <summary>T034 (FR-012…FR-014): errores sin detalles internos, cabeceras comunes y CORS solo para el panel.</summary>
public sealed class ErroresYCabecerasTests(BaseDatosPrueba bd) : IDisposable
{
    private readonly FabricaApi _api = new(bd);
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Una_excepcion_es_500_error_interno_sin_traza()
    {
        using var respuesta = await _api.CreateClient().GetAsync("/api/v1/prueba/error", Ct);
        var cuerpo = await respuesta.Content.ReadAsStringAsync(Ct);

        await SeguridadTests.AfirmarProblemaAsync(respuesta, HttpStatusCode.InternalServerError, "error_interno");
        Assert.DoesNotContain("detalle interno", cuerpo, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidOperationException", cuerpo, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/api/v1/prueba/publica")]
    [InlineData("/api/v1/prueba/error")]
    [InlineData("/health/live")]
    public async Task Toda_respuesta_lleva_las_cabeceras_comunes(string ruta)
    {
        using var respuesta = await _api.CreateClient().GetAsync(ruta, Ct);

        var cache = respuesta.Headers.CacheControl!;
        Assert.True(cache.Private && cache.NoStore && !cache.Public, $"Cache-Control: {cache}");
        Assert.Contains("max-age=31536000", respuesta.Headers.GetValues("Strict-Transport-Security").Single(), StringComparison.Ordinal);
        Assert.Equal("nosniff", respuesta.Headers.GetValues("X-Content-Type-Options").Single());
    }

    [Theory]
    [InlineData("http://localhost:5173", true)]   // OrigenPanel en Development
    [InlineData("https://sitio-ajeno.com", false)]
    public async Task CORS_solo_para_el_origen_del_panel(string origen, bool permitido)
    {
        using var peticion = new HttpRequestMessage(HttpMethod.Options, "/api/v1/prueba/publica");
        peticion.Headers.Add("Origin", origen);
        peticion.Headers.Add("Access-Control-Request-Method", "GET");

        using var respuesta = await _api.CreateClient().SendAsync(peticion, Ct);

        Assert.Equal(permitido, respuesta.Headers.Contains("Access-Control-Allow-Origin"));
    }

    public void Dispose() => _api.Dispose();
}
