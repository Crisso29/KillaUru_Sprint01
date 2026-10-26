using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;
using YamlDotNet.RepresentationModel;
namespace KillaUru.Integracion.Tests.Contratos;
/// <summary>Compara el contrato escrito con el generado, incluidas las seis operaciones y la fila exportada.</summary>
public sealed class ContratoAnomaliasTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Esquemas_y_operaciones_respetan_la_spec()
    {
        using var api = new FabricaApi(bd); var ct = TestContext.Current.CancellationToken;
        using var json = JsonDocument.Parse(await api.CreateClient().GetStringAsync("/openapi/v1.json", ct));
        using var lector = File.OpenText(Path.Combine(Rutas.Specs, "004-modulo-anomalias/contracts/anomalias.openapi.yaml"));
        var yaml = new YamlStream(); yaml.Load(lector); var raiz = (YamlMappingNode)yaml.Documents[0].RootNode;
        var comparador = new ComparadorEsquemas(raiz, json.RootElement);
        var generados = json.RootElement.GetProperty("components").GetProperty("schemas");
        Assert.Contains(generados.GetProperty("SolicitudExportacion").GetProperty("properties").GetProperty("tipo").GetProperty("oneOf").EnumerateArray(),
            opcion => opcion.TryGetProperty("type", out var tipo) && tipo.GetString() == "null");
        var esquemas = (YamlMappingNode)((YamlMappingNode)raiz.Children["components"]).Children["schemas"];
        foreach (var nombre in new[] { "DetalleAnomalia", "ElementoPorRevisar", "ElementoPendiente", "TransicionReciente", "SolicitudExportacion", "FilaExportacion" })
            comparador.Comparar((YamlMappingNode)esquemas.Children[nombre], json.RootElement.GetProperty("components").GetProperty("schemas").GetProperty(nombre));
        foreach (var (ruta, nodo) in ((YamlMappingNode)raiz.Children["paths"]).Children)
        {
            foreach (var (metodo, operacion) in ((YamlMappingNode)nodo).Children)
            {
                var actual = json.RootElement.GetProperty("paths").GetProperty(ruta.ToString()).GetProperty(metodo.ToString());
                Assert.Equal(((YamlMappingNode)operacion).Children["operationId"].ToString(), actual.GetProperty("operationId").GetString());
                foreach (var codigo in new[] { "401", "403", "429" }) Assert.True(actual.GetProperty("responses").TryGetProperty(codigo, out _));
            }
        }
    }
}
