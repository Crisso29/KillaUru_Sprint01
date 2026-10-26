using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;
using YamlDotNet.RepresentationModel;

namespace KillaUru.Integracion.Tests.Contratos;

/// <summary>
/// Principio IX: el OpenAPI publicado por la API coincide con el contrato escrito antes de programar
/// (`specs/004/contracts/anomalias.openapi.yaml`), incluidas las seis operaciones y la fila exportada.
/// Spec 004 · FR-025, FR-027, FR-028 (001 FR-042).
/// </summary>
public sealed class ContratoAnomaliasTests(BaseDatosPrueba bd)
{
    private static readonly string[] EsquemasDelContrato =
        ["DetalleAnomalia", "ElementoPorRevisar", "ElementoPendiente", "TransicionReciente", "SolicitudExportacion", "FilaExportacion"];

    private static readonly string[] RespuestasObligatorias = ["401", "403", "429"];

    private sealed record Documentos(YamlMappingNode Contrato, JsonDocument Json) : IDisposable
    {
        internal JsonElement Publicado => Json.RootElement;
        internal JsonElement EsquemasPublicados => Publicado.GetProperty("components").GetProperty("schemas");
        public void Dispose() => Json.Dispose();
    }

    private async Task<Documentos> CargarAsync()
    {
        using var api = new FabricaApi(bd);
        var publicado = await api.CreateClient().GetStringAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        using var lector = File.OpenText(Path.Combine(Rutas.Specs, "004-modulo-anomalias/contracts/anomalias.openapi.yaml"));
        var yaml = new YamlStream();
        yaml.Load(lector);
        return new((YamlMappingNode)yaml.Documents[0].RootNode, JsonDocument.Parse(publicado));
    }

    private static YamlMappingNode Hijo(YamlMappingNode nodo, string clave) => (YamlMappingNode)nodo.Children[clave];

    /// <summary>Cada esquema del contrato existe en la API con las mismas propiedades y tipos.</summary>
    [Fact]
    public async Task Esquemas_publicados_coinciden_con_el_contrato()
    {
        // Arrange
        using var documentos = await CargarAsync();
        var comparador = new ComparadorEsquemas(documentos.Contrato, documentos.Publicado);
        var esquemasContrato = Hijo(Hijo(documentos.Contrato, "components"), "schemas");

        // Act + Assert — el comparador falla con el nombre de la propiedad que difiere
        foreach (var nombre in EsquemasDelContrato)
            comparador.Comparar((YamlMappingNode)esquemasContrato.Children[nombre], documentos.EsquemasPublicados.GetProperty(nombre));
    }

    /// <summary>El filtro de tipo de la exportación acepta null: exportar todos los tipos (FR-028).</summary>
    [Fact]
    public async Task Tipo_de_la_exportacion_es_opcional()
    {
        // Arrange
        using var documentos = await CargarAsync();

        // Act
        var opciones = documentos.EsquemasPublicados.GetProperty("SolicitudExportacion").GetProperty("properties").GetProperty("tipo").GetProperty("oneOf");

        // Assert
        Assert.Contains(opciones.EnumerateArray(), opcion => opcion.TryGetProperty("type", out var tipo) && tipo.GetString() == "null");
    }

    /// <summary>Toda operación conserva su operationId y declara 401, 403 y 429 (001 FR-042).</summary>
    [Fact]
    public async Task Cada_operacion_conserva_su_id_y_declara_401_403_y_429()
    {
        // Arrange
        using var documentos = await CargarAsync();
        var rutasPublicadas = documentos.Publicado.GetProperty("paths");

        // Act + Assert
        foreach (var (ruta, nodo) in Hijo(documentos.Contrato, "paths").Children)
        {
            foreach (var (metodo, operacion) in ((YamlMappingNode)nodo).Children)
            {
                var publicada = rutasPublicadas.GetProperty(ruta.ToString()).GetProperty(metodo.ToString());
                Assert.Equal(((YamlMappingNode)operacion).Children["operationId"].ToString(), publicada.GetProperty("operationId").GetString());
                foreach (var codigo in RespuestasObligatorias)
                    Assert.True(publicada.GetProperty("responses").TryGetProperty(codigo, out _), $"{metodo} {ruta} no declara {codigo}");
            }
        }
    }
}
