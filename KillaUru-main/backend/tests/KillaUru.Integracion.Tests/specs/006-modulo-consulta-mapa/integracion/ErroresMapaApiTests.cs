using System.Net;
using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Mapa;

/// <summary>
/// Los errores del mapa son <c>application/problem+json</c> con <c>codigo</c>, como dice el contrato
/// (`mapa.openapi.yaml`, 400) y la constitución IX. Encontrado con la colección de Postman (2026-11-23): el
/// <c>[Produces("application/geo+json")]</c> de la acción reemplazaba el tipo de TODAS sus respuestas, también las de
/// error, y el cliente recibía un problema etiquetado como GeoJSON. Spec 006 · FR-001.
/// </summary>
public sealed class ErroresMapaApiTests(BaseDatosPrueba bd)
{
    [Theory]
    [InlineData("bbox=-74.35,-13.25,-74.20", "datos_invalidos")]
    [InlineData("updatedSince=2026-11-01T00:00:00Z&tipo=Bache", "parametros_incompatibles")]
    public async Task Un_parametro_invalido_responde_un_problema_y_no_GeoJSON(string consulta, string codigo)
    {
        // Arrange
        using var api = new FabricaApi(bd);

        // Act
        using var respuesta = await api.CreateClient().GetAsync($"/api/v1/anomalies?{consulta}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        using var problema = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(codigo, problema.RootElement.GetProperty("codigo").GetString());
    }
}
