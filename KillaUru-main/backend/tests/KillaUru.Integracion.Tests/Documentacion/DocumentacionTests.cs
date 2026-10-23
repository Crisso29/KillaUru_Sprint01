using System.Net;
using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Documentacion;

/// <summary>T041 (FR-015): la documentación existe en Development y no existe en producción.</summary>
public sealed class DocumentacionTests(BaseDatosPrueba bd)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task En_Development_hay_OpenAPI_con_la_salud_y_hay_docs()
    {
        using var api = new FabricaApi(bd);
        var cliente = api.CreateClient();

        var documento = await cliente.GetStringAsync("/openapi/v1.json", Ct);
        using var docs = await cliente.GetAsync("/docs", Ct);

        Assert.Contains("/health/live", documento, StringComparison.Ordinal);
        Assert.Contains("/health/ready", documento, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, docs.StatusCode);
    }

    [Theory]
    [InlineData("/openapi/v1.json")]
    [InlineData("/docs")]
    public async Task En_produccion_no_hay_documentacion(string ruta)
    {
        using var api = new FabricaApi(bd, "Production");

        using var respuesta = await api.CreateClient().GetAsync(ruta, Ct);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }
}
