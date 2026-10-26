using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;
using YamlDotNet.RepresentationModel;
namespace KillaUru.Integracion.Tests.Contratos;

public sealed class ContratoIdentidadTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Contrato_respeta_operaciones_esquemas_y_variantes_de_sesion()
    {
        using var api = new FabricaApi(bd);
        var documento = await api.CreateClient().GetStringAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        Directory.CreateDirectory("TestResults");
        await File.WriteAllTextAsync("TestResults/identidad.openapi.json", documento, TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(documento);
        using var lector = File.OpenText(Path.Combine(Rutas.Specs, "003-modulo-identidad-accesos/contracts/identidad.openapi.yaml"));
        var yaml = new YamlStream(); yaml.Load(lector);
        var raiz = (YamlMappingNode)yaml.Documents[0].RootNode;
        var comparador = new ComparadorEsquemas(raiz, json.RootElement);
        var esquemas = (YamlMappingNode)((YamlMappingNode)raiz.Children["components"]).Children["schemas"];
        foreach (var nombre in new[] { "VersionesLegales", "RegistroSolicitud", "InicioSesionSolicitud", "RenovarSolicitud",
            "SesionApp", "SesionPanel", "TokenEnlace", "MisDatos", "Usuario", "PaginaUsuarios", "EstadoRevision" })
        {
            comparador.Comparar((YamlMappingNode)esquemas.Children[nombre], json.RootElement.GetProperty("components").GetProperty("schemas").GetProperty(nombre));
        }
        foreach (var (ruta, nodo) in ((YamlMappingNode)raiz.Children["paths"]).Children)
        {
            foreach (var (metodo, operacion) in ((YamlMappingNode)nodo).Children)
            {
                var actual = json.RootElement.GetProperty("paths").GetProperty(ruta.ToString()).GetProperty(metodo.ToString());
                Assert.Equal(((YamlMappingNode)operacion).Children["operationId"].ToString(), actual.GetProperty("operationId").GetString());
                Assert.True(actual.GetProperty("responses").TryGetProperty("429", out _));
            }
        }
        foreach (var ruta in new[] { "inicio-sesion", "renovar" })
        {
            Assert.Equal(2, json.RootElement.GetProperty("paths").GetProperty("/api/v1/auth/" + ruta).GetProperty("post")
                .GetProperty("responses").GetProperty("200").GetProperty("content").GetProperty("application/json").GetProperty("schema").GetProperty("oneOf").GetArrayLength());
        }
    }
}
