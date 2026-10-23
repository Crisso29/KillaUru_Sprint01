using System.Net;
using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.AspNetCore.Hosting;
namespace KillaUru.Integracion.Tests.Seguridad;

/// <summary>Regresiones de 001 necesarias al incorporar el primer endpoint de negocio.</summary>
public sealed class PendientesPlataformaTests(BaseDatosPrueba bd)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Production_no_publica_docs_aunque_se_active_la_opcion()
    {
        using var original = new FabricaApi(bd, "Production");
        using var api = original.WithWebHostBuilder(b => b.UseSetting("Api:DocumentacionPublica", "true"));
        Assert.Equal(HttpStatusCode.NotFound, (await api.CreateClient().GetAsync("/openapi/v1.json", Ct)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.CreateClient().GetAsync("/docs", Ct)).StatusCode);
    }

    [Fact]
    public async Task Ruta_inexistente_devuelve_problem_details()
    {
        using var api = new FabricaApi(bd);
        using var respuesta = await api.CreateClient().GetAsync("/ruta-inexistente", Ct);
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        using var cuerpo = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(Ct));
        Assert.Equal("no_encontrado", cuerpo.RootElement.GetProperty("codigo").GetString());
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Solo_anonimos_conservan_cache_publica(bool autenticado)
    {
        using var api = new FabricaApi(bd);
        using var cliente = api.CreateClient();
        if (autenticado) cliente.DefaultRequestHeaders.Authorization = new("Bearer", Tokens.Emitir("Administrador"));
        using var respuesta = await cliente.GetAsync("/api/v1/prueba/cache-publica", Ct);
        Assert.Equal(!autenticado, respuesta.Headers.CacheControl!.Public);
        Assert.Equal(autenticado, respuesta.Headers.CacheControl.NoStore);
    }
}
