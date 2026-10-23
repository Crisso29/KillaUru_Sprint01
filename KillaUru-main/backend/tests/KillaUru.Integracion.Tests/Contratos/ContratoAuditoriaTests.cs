using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;
using YamlDotNet.RepresentationModel;
namespace KillaUru.Integracion.Tests.Contratos;

/// <summary>Comprueba esquemas, filtros, credenciales alternativas, errores y ejemplo de 002.</summary>
public sealed class ContratoAuditoriaTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task El_documento_generado_respeta_el_contrato_de_auditoria()
    {
        using var api = new FabricaApi(bd);
        using var json = JsonDocument.Parse(await api.CreateClient().GetStringAsync("/openapi/v1.json", TestContext.Current.CancellationToken));
        using var lector = File.OpenText(Path.Combine(Rutas.Specs, "002-modulo-auditoria/contracts/auditoria.openapi.yaml"));
        var yaml = new YamlStream();
        yaml.Load(lector);
        var raiz = (YamlMappingNode)yaml.Documents[0].RootNode;
        var comparador = new ComparadorEsquemas(raiz, json.RootElement);
        var esquemas = (YamlMappingNode)((YamlMappingNode)raiz.Children["components"]).Children["schemas"];
        comparador.Comparar((YamlMappingNode)esquemas.Children["PaginaAuditoria"],
            json.RootElement.GetProperty("components").GetProperty("schemas").GetProperty("PaginaAuditoria"));
        VerificarOperacion(raiz, json.RootElement, comparador);
    }

    private static void VerificarOperacion(YamlMappingNode raiz, JsonElement generado, ComparadorEsquemas comparador)
    {
        var operacion = generado.GetProperty("paths").GetProperty("/api/v1/auditoria").GetProperty("get");
        var esperado = (YamlMappingNode)((YamlMappingNode)((YamlMappingNode)raiz.Children["paths"]).Children["/api/v1/auditoria"]).Children["get"];
        foreach (var nodo in (YamlSequenceNode)esperado.Children["parameters"])
        {
            var parametro = (YamlMappingNode)nodo;
            var actual = operacion.GetProperty("parameters").EnumerateArray().Single(p => p.GetProperty("name").GetString() == parametro.Children["name"].ToString());
            comparador.Comparar((YamlMappingNode)parametro.Children["schema"], actual.GetProperty("schema"));
        }
        Assert.Equal(["cookieAcceso", "tokenAcceso"],
            operacion.GetProperty("security").EnumerateArray().SelectMany(s => s.EnumerateObject().Select(p => p.Name)).Order());
        Assert.True(operacion.GetProperty("responses").GetProperty("429").GetProperty("headers").TryGetProperty("Retry-After", out _));
        Assert.True(operacion.GetProperty("responses").GetProperty("400").GetProperty("content").TryGetProperty("application/problem+json", out _));
        Assert.True(operacion.GetProperty("responses").GetProperty("200").GetProperty("content").GetProperty("application/json").TryGetProperty("example", out _));
    }
}
