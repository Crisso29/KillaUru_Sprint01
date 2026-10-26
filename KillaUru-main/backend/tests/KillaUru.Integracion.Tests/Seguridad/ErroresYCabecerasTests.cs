using System.Net;
using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Seguridad;

/// <summary>
/// Spec 001 — FR-012, FR-013, FR-014 (T034):
/// todas las respuestas llevan cabeceras de seguridad; los errores son ProblemDetails
/// sin trazas internas; CORS solo permite el origen del panel.
/// </summary>
public sealed class ErroresYCabecerasTests(BaseDatosPrueba bd) : IDisposable
{
    private readonly FabricaApi _api = new(bd);
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Una_excepcion_no_manejada_expone_500_sin_traza_interna()
    {
        // Arrange — endpoint que lanza una excepción deliberada con mensaje sensible

        // Act
        using var respuesta = await _api.CreateClient().GetAsync("/api/v1/prueba/error", Ct);
        var cuerpo = await respuesta.Content.ReadAsStringAsync(Ct);

        // Assert — ProblemDetails con código estable y sin datos del stack (FR-012)
        await SeguridadTests.AfirmarProblemaAsync(respuesta, HttpStatusCode.InternalServerError, "error_interno");
        Assert.DoesNotContain("detalle interno", cuerpo, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidOperationException", cuerpo, StringComparison.Ordinal);
    }

    /// <summary>
    /// Cache-Control: private, no-store (ADR-006), HSTS y X-Content-Type-Options
    /// son obligatorias en toda respuesta, no solo en las exitosas (FR-013, FR-014).
    /// </summary>
    [Theory]
    [InlineData("/api/v1/prueba/publica")]
    [InlineData("/api/v1/prueba/error")]
    [InlineData("/health/live")]
    public async Task Toda_respuesta_lleva_cache_control_HSTS_y_nosniff(string ruta)
    {
        // Arrange — petición sin token (rol Anónimo)

        // Act
        using var respuesta = await _api.CreateClient().GetAsync(ruta, Ct);

        // Assert — cabeceras de seguridad presentes en todas las rutas (FR-013, FR-014)
        var cache = respuesta.Headers.CacheControl!;
        Assert.True(cache.Private && cache.NoStore && !cache.Public,
            $"Se esperaba 'private, no-store' en Cache-Control pero se recibió: {cache}");
        Assert.Contains("max-age=31536000",
            respuesta.Headers.GetValues("Strict-Transport-Security").Single(), StringComparison.Ordinal);
        Assert.Equal("nosniff", respuesta.Headers.GetValues("X-Content-Type-Options").Single());
    }

    /// <summary>
    /// CORS solo permite el origen del panel para evitar peticiones cross-site de otros sitios.
    /// En Development el origen del panel es http://localhost:5173 (FR-014, RNF-08/09).
    /// </summary>
    [Theory]
    [InlineData("http://localhost:5173", true)]
    [InlineData("https://sitio-ajeno.com", false)]
    public async Task CORS_solo_responde_al_origen_del_panel_con_credenciales(string origen, bool permitido)
    {
        // Arrange — preflight OPTIONS como haría un navegador
        using var peticion = new HttpRequestMessage(HttpMethod.Options, "/api/v1/prueba/publica");
        peticion.Headers.Add("Origin", origen);
        peticion.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        using var respuesta = await _api.CreateClient().SendAsync(peticion, Ct);

        // Assert — el origin del panel recibe Allow-Origin; el ajeno no (FR-014)
        Assert.Equal(permitido, respuesta.Headers.Contains("Access-Control-Allow-Origin"));
    }

    public void Dispose() => _api.Dispose();
}
