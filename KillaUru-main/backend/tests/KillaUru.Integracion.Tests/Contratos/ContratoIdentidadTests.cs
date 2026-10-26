using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;
using YamlDotNet.RepresentationModel;

namespace KillaUru.Integracion.Tests.Contratos;

/// <summary>
/// Principio IX: el OpenAPI que publica la API coincide con el contrato escrito antes de programar
/// (`specs/003/contracts/identidad.openapi.yaml`). Spec 003 · FR-020, FR-042.
/// </summary>
public sealed class ContratoIdentidadTests(BaseDatosPrueba bd)
{
    private static readonly string[] EsquemasDelContrato =
    [
        "VersionesLegales", "RegistroSolicitud", "InicioSesionSolicitud", "RenovarSolicitud", "SesionApp",
        "SesionPanel", "TokenEnlace", "MisDatos", "Usuario", "PaginaUsuarios", "EstadoRevision",
    ];

    /// <summary>Contrato escrito (YAML) y documento publicado por la API (JSON).</summary>
    private sealed record Documentos(YamlMappingNode Contrato, JsonDocument Json) : IDisposable
    {
        internal JsonElement Publicado => Json.RootElement;
        public void Dispose() => Json.Dispose();
    }

    private async Task<Documentos> CargarAsync()
    {
        using var api = new FabricaApi(bd);
        var publicado = await api.CreateClient().GetStringAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        // Se guarda en TestResults para revisar a mano el documento real si el contrato falla.
        Directory.CreateDirectory("TestResults");
        await File.WriteAllTextAsync("TestResults/identidad.openapi.json", publicado, TestContext.Current.CancellationToken);
        using var lector = File.OpenText(Path.Combine(Rutas.Specs, "003-modulo-identidad-accesos/contracts/identidad.openapi.yaml"));
        var yaml = new YamlStream();
        yaml.Load(lector);
        return new Documentos((YamlMappingNode)yaml.Documents[0].RootNode, JsonDocument.Parse(publicado));
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
        var esquemasPublicados = documentos.Publicado.GetProperty("components").GetProperty("schemas");

        // Act + Assert — el comparador falla con el nombre de la propiedad que difiere
        foreach (var nombre in EsquemasDelContrato)
            comparador.Comparar((YamlMappingNode)esquemasContrato.Children[nombre], esquemasPublicados.GetProperty(nombre));
    }

    /// <summary>Toda operación conserva su operationId (lo usan los clientes generados) y declara 429 (FR-042).</summary>
    [Fact]
    public async Task Cada_operacion_conserva_su_id_y_declara_429()
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
                Assert.True(publicada.GetProperty("responses").TryGetProperty("429", out _), $"{metodo} {ruta} no declara 429");
            }
        }
    }

    /// <summary>Inicio y renovación responden dos formas: con tokens (app) o sin ellos, solo cookies (panel) (FR-020).</summary>
    [Theory]
    [InlineData("inicio-sesion")]
    [InlineData("renovar")]
    public async Task Sesion_ofrece_variante_app_y_panel(string ruta)
    {
        // Arrange
        using var documentos = await CargarAsync();

        // Act
        var respuesta200 = documentos.Publicado.GetProperty("paths").GetProperty("/api/v1/auth/" + ruta).GetProperty("post")
            .GetProperty("responses").GetProperty("200").GetProperty("content").GetProperty("application/json").GetProperty("schema");

        // Assert
        Assert.Equal(2, respuesta200.GetProperty("oneOf").GetArrayLength());
    }
}
