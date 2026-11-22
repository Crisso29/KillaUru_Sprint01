using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;
using YamlDotNet.RepresentationModel;

namespace KillaUru.Integracion.Tests.Contratos;

/// <summary>
/// Principio IX: el OpenAPI publicado por la API coincide con el contrato escrito antes de programar
/// (`specs/006/contracts/mapa.openapi.yaml`). Spec 006 · T029 · FR-001 (001 FR-042).
/// </summary>
public sealed class ContratoMapaTests(BaseDatosPrueba bd)
{
    private const string Ruta = "/api/v1/anomalies";
    private const string Archivo = "006-modulo-consulta-mapa/contracts/mapa.openapi.yaml";

    private static readonly string[] EsquemasDelContrato = ["ColeccionAnomalias", "AnomaliaFeature", "BajaFeature"];

    private sealed record Documentos(YamlMappingNode Contrato, JsonDocument Json) : IDisposable
    {
        internal JsonElement Publicado => Json.RootElement;
        internal JsonElement Operacion => Publicado.GetProperty("paths").GetProperty(Ruta).GetProperty("get");
        internal YamlMappingNode OperacionContrato => Hijo(Hijo(Hijo(Contrato, "paths"), Ruta), "get");
        public void Dispose() => Json.Dispose();
    }

    private async Task<Documentos> CargarAsync()
    {
        using var api = new FabricaApi(bd);
        var publicado = await api.CreateClient().GetStringAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        using var lector = File.OpenText(Path.Combine(Rutas.Specs, Archivo));
        var yaml = new YamlStream();
        yaml.Load(lector);
        return new((YamlMappingNode)yaml.Documents[0].RootNode, JsonDocument.Parse(publicado));
    }

    private static YamlMappingNode Hijo(YamlMappingNode nodo, string clave) => (YamlMappingNode)nodo.Children[clave];

    /// <summary>La operación conserva el operationId del contrato: la app y el panel dependen de él.</summary>
    [Fact]
    public async Task Consulta_del_mapa_conserva_el_operationId_consultarMapa()
    {
        // Arrange
        using var documentos = await CargarAsync();

        // Act
        var operationId = documentos.Operacion.GetProperty("operationId").GetString();

        // Assert
        Assert.Equal("consultarMapa", operationId);
    }

    /// <summary>Se publican 200, 304 (ETag), 400, 401 (token inválido aunque sea pública), 403 y 429.</summary>
    [Fact]
    public async Task Consulta_del_mapa_declara_todos_los_codigos_del_contrato()
    {
        // Arrange
        using var documentos = await CargarAsync();
        var esperados = ContratoYaml.Leer(Path.Combine(Rutas.Specs, Archivo)).Single(o => o.Ruta == Ruta).Codigos;

        // Act
        var publicados = documentos.Operacion.GetProperty("responses").EnumerateObject().Select(r => r.Name).ToHashSet();

        // Assert
        Assert.Superset(esperados.ToHashSet(), publicados);
    }

    /// <summary>El 200 es GeoJSON (`application/geo+json`) con la colección del contrato.</summary>
    [Fact]
    public async Task Respuesta_200_es_geojson_con_ColeccionAnomalias()
    {
        // Arrange
        using var documentos = await CargarAsync();

        // Act
        var contenido = documentos.Operacion.GetProperty("responses").GetProperty("200").GetProperty("content");

        // Assert
        var esquema = contenido.GetProperty("application/geo+json").GetProperty("schema");
        Assert.Equal("#/components/schemas/ColeccionAnomalias", esquema.GetProperty("$ref").GetString());
    }

    /// <summary>Colección, anomalía (con `kmVia`) y baja existen con las mismas propiedades, tipos y límites.</summary>
    [Fact]
    public async Task Esquemas_publicados_coinciden_con_el_contrato()
    {
        // Arrange
        using var documentos = await CargarAsync();
        var comparador = new ComparadorEsquemas(documentos.Contrato, documentos.Publicado);
        var esquemasContrato = Hijo(Hijo(documentos.Contrato, "components"), "schemas");
        var publicados = documentos.Publicado.GetProperty("components").GetProperty("schemas");

        // Act + Assert — el comparador falla con la restricción que difiere
        foreach (var nombre in EsquemasDelContrato)
            comparador.Comparar((YamlMappingNode)esquemasContrato.Children[nombre], publicados.GetProperty(nombre));
    }

    /// <summary>
    /// Cada parámetro de consulta existe con el tipo del contrato: `severidad` y `estado` son listas
    /// separadas por comas (`4,5`), no parámetros repetidos.
    /// </summary>
    [Fact]
    public async Task Parametros_de_consulta_coinciden_con_el_contrato()
    {
        // Arrange
        using var documentos = await CargarAsync();
        var comparador = new ComparadorEsquemas(documentos.Contrato, documentos.Publicado);
        var publicados = documentos.Operacion.GetProperty("parameters").EnumerateArray()
            .ToDictionary(p => p.GetProperty("name").GetString()!, p => p.GetProperty("schema"));
        var delContrato = ((YamlSequenceNode)documentos.OperacionContrato.Children["parameters"]).Children
            .OfType<YamlMappingNode>().Where(p => p.Children.TryGetValue("in", out var en) && en.ToString() == "query");

        // Act + Assert
        foreach (var parametro in delContrato)
        {
            var nombre = parametro.Children["name"].ToString();
            Assert.True(publicados.TryGetValue(nombre, out var esquema), $"Falta el parámetro {nombre}");
            comparador.Comparar(Hijo(parametro, "schema"), esquema);
        }
    }
}
