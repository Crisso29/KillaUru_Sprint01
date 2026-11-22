using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;
using YamlDotNet.RepresentationModel;

namespace KillaUru.Integracion.Tests.Contratos;

/// <summary>
/// Principio IX: el OpenAPI publicado por la API coincide con el contrato escrito antes de programar
/// (`specs/005/contracts/ingesta.openapi.yaml`). Spec 005 · T034 · FR-001, FR-006 (001 FR-042).
/// </summary>
public sealed class ContratoIngestaTests(BaseDatosPrueba bd)
{
    private const string Ruta = "/api/v1/anomalies/batch";

    private static readonly string[] EsquemasDelContrato = ["Lote", "RespuestaLote"];

    private sealed record Documentos(YamlMappingNode Contrato, JsonDocument Json) : IDisposable
    {
        internal JsonElement Publicado => Json.RootElement;
        internal JsonElement Operacion => Publicado.GetProperty("paths").GetProperty(Ruta).GetProperty("post");
        internal JsonElement EsquemasPublicados => Publicado.GetProperty("components").GetProperty("schemas");
        public void Dispose() => Json.Dispose();
    }

    private async Task<Documentos> CargarAsync()
    {
        using var api = new FabricaApi(bd);
        var publicado = await api.CreateClient().GetStringAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        using var lector = File.OpenText(Path.Combine(Rutas.Specs, "005-modulo-ingesta/contracts/ingesta.openapi.yaml"));
        var yaml = new YamlStream();
        yaml.Load(lector);
        return new((YamlMappingNode)yaml.Documents[0].RootNode, JsonDocument.Parse(publicado));
    }

    private static YamlMappingNode Hijo(YamlMappingNode nodo, string clave) => (YamlMappingNode)nodo.Children[clave];

    /// <summary>La operación conserva el operationId del contrato: los clientes generados dependen de él.</summary>
    [Fact]
    public async Task Envio_de_lote_conserva_el_operationId_recibirLote()
    {
        // Arrange
        using var documentos = await CargarAsync();

        // Act
        var operationId = documentos.Operacion.GetProperty("operationId").GetString();

        // Assert
        Assert.Equal("recibirLote", operationId);
    }

    /// <summary>Se publican todos los códigos del contrato, incluido 413 por cuerpo de más de 256 KB (FR-001).</summary>
    [Fact]
    public async Task Envio_de_lote_declara_todos_los_codigos_del_contrato()
    {
        // Arrange
        using var documentos = await CargarAsync();
        var esperados = ContratoYaml.Leer(Path.Combine(Rutas.Specs, "005-modulo-ingesta/contracts/ingesta.openapi.yaml"))
            .Single(o => o.Ruta == Ruta).Codigos;

        // Act
        var publicados = documentos.Operacion.GetProperty("responses").EnumerateObject().Select(r => r.Name).ToHashSet();

        // Assert — 200, 400, 401, 403, 413, 429 y 503
        Assert.Superset(esperados.ToHashSet(), publicados);
    }

    /// <summary>Lote, Evento y RespuestaLote existen con las mismas propiedades, tipos y límites (FR-006).</summary>
    [Fact]
    public async Task Esquemas_publicados_coinciden_con_el_contrato()
    {
        // Arrange
        using var documentos = await CargarAsync();
        var comparador = new ComparadorEsquemas(documentos.Contrato, documentos.Publicado);
        var esquemasContrato = Hijo(Hijo(documentos.Contrato, "components"), "schemas");

        // Act + Assert — el comparador sigue `items` (Evento, rechazados) y falla con la restricción que difiere
        foreach (var nombre in EsquemasDelContrato)
            comparador.Comparar((YamlMappingNode)esquemasContrato.Children[nombre], documentos.EsquemasPublicados.GetProperty(nombre));
    }

    /// <summary>El 503 por cola saturada indica cuándo reintentar con la cabecera Retry-After.</summary>
    [Fact]
    public async Task Cola_saturada_503_declara_Retry_After()
    {
        // Arrange
        using var documentos = await CargarAsync();

        // Act
        var respuesta = documentos.Operacion.GetProperty("responses").GetProperty("503");

        // Assert
        Assert.True(respuesta.GetProperty("headers").TryGetProperty("Retry-After", out _));
    }
}
