using System.Net;
using System.Text;
using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// Ninguna operación administrativa de Anomalías queda expuesta: sin token responde 401 y con un
/// token de conductor responde 403. Spec 004 · FR-025, FR-027, FR-028 (001 FR-042).
/// </summary>
[Collection("Anomalias")]
public sealed class SeguridadAnomaliasTests(BaseDatosPrueba bd)
{
    private const string IdCualquiera = "00000000-0000-0000-0000-000000000001";

    /// <summary>Las lecturas del panel exigen sesión.</summary>
    [Theory]
    [InlineData("/api/v1/moderacion/pendientes")]
    [InlineData("/api/v1/moderacion/por-revisar")]
    [InlineData("/api/v1/anomalies/transiciones")]
    [InlineData("/api/v1/anomalies/" + IdCualquiera)]
    public async Task Sin_token_responde_401(string ruta)
    {
        // Arrange
        using var api = new FabricaApi(bd);
        using var cliente = api.CreateClient();

        // Act
        using var respuesta = await cliente.GetAsync(ruta, EscenarioAnomalias.Ct);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    /// <summary>Un conductor autenticado no puede leer, moderar ni exportar.</summary>
    [Theory]
    [InlineData("GET", "/api/v1/moderacion/pendientes")]
    [InlineData("GET", "/api/v1/moderacion/por-revisar")]
    [InlineData("GET", "/api/v1/anomalies/transiciones")]
    [InlineData("GET", "/api/v1/anomalies/" + IdCualquiera)]
    [InlineData("POST", "/api/v1/anomalies/" + IdCualquiera + "/moderacion")]
    [InlineData("POST", "/api/v1/exportaciones")]
    public async Task Conductor_recibe_403_en_toda_operacion(string metodo, string ruta)
    {
        // Arrange
        using var api = new FabricaApi(bd);
        using var cliente = api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", Tokens.Emitir("ConductorRegistrado"));
        using var peticion = new HttpRequestMessage(new HttpMethod(metodo), ruta)
        {
            Content = metodo == "POST" ? new StringContent("{}", Encoding.UTF8, "application/json") : null,
        };

        // Act
        using var respuesta = await cliente.SendAsync(peticion, EscenarioAnomalias.Ct);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }
}
