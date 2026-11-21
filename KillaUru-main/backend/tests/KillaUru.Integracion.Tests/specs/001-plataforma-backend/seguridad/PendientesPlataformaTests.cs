using System.Net;
using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.AspNetCore.Hosting;

namespace KillaUru.Integracion.Tests.Seguridad;

/// <summary>
/// Spec 001 — FR-015, FR-016, FR-017 (T035, T036, T037, RNF-07):
/// regresiones de plataforma que deben mantenerse al incorporar cualquier endpoint de negocio.
/// Valida que la configuración de producción, los errores y la caché sean correctos.
/// </summary>
public sealed class PendientesPlataformaTests(BaseDatosPrueba bd)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Production_no_publica_docs_aunque_se_active_la_opcion()
    {
        // Arrange — entorno Production con la bandera de documentación activada deliberadamente
        using var original = new FabricaApi(bd, "Production");
        using var api = original.WithWebHostBuilder(b => b.UseSetting("Api:DocumentacionPublica", "true"));
        var cliente = api.CreateClient();

        // Act
        using var openapi = await cliente.GetAsync("/openapi/v1.json", Ct);
        using var docs    = await cliente.GetAsync("/docs", Ct);

        // Assert — en producción la documentación nunca se expone, aunque la opción diga "true" (FR-015)
        Assert.Equal(HttpStatusCode.NotFound, openapi.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, docs.StatusCode);
    }

    [Fact]
    public async Task Ruta_inexistente_devuelve_problem_details_con_codigo_estable()
    {
        // Arrange — petición a una ruta que no existe en ningún módulo
        using var api = new FabricaApi(bd);
        using var respuesta = await api.CreateClient().GetAsync("/ruta-inexistente", Ct);

        // Act
        using var cuerpo = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(Ct));

        // Assert — 404 con ProblemDetails y código estable que el cliente puede evaluar (FR-012)
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Equal("no_encontrado", cuerpo.RootElement.GetProperty("codigo").GetString());
    }

    /// <summary>
    /// Los anónimos pueden beneficiarse de la caché del proxy (CDN) para reducir latencia.
    /// Los usuarios autenticados no deben quedar expuestos en cachés intermedios (ADR-006, FR-017).
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Solo_anonimos_conservan_cache_publica(bool autenticado)
    {
        // Arrange — cliente con o sin token según el parámetro
        using var api = new FabricaApi(bd);
        using var cliente = api.CreateClient();
        if (autenticado)
            cliente.DefaultRequestHeaders.Authorization = new("Bearer", Tokens.Emitir("Administrador"));

        // Act
        using var respuesta = await cliente.GetAsync("/api/v1/prueba/cache-publica", Ct);

        // Assert — anónimo obtiene Cache-Control: public; autenticado obtiene no-store (FR-017, ADR-006)
        Assert.Equal(!autenticado, respuesta.Headers.CacheControl!.Public);
        Assert.Equal(autenticado,  respuesta.Headers.CacheControl.NoStore);
    }
}
