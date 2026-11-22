using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace KillaUru.Mapa.Adapters.Documentacion;

/// <summary>
/// Esquemas GeoJSON del contrato `specs/006/contracts/mapa.openapi.yaml`. Se escriben a mano porque el
/// controlador devuelve bytes preserializados por la caché de celdas (ADR-006), no un tipo C#.
/// </summary>
internal static class EsquemasMapa
{
    internal static OpenApiSchema Catalogo(params string[] valores) => new()
    {
        Type = JsonSchemaType.String,
        Enum = valores.Select(v => (JsonNode)JsonValue.Create(v)!).ToList(),
    };

    internal static OpenApiSchema Tipos() => Catalogo("Bache", "Hundimiento");

    internal static OpenApiSchema Estados() =>
        Catalogo("Activa", "PendienteConfirmacion", "Expirada", "Reparada", "Descartada");

    internal static OpenApiSchema Coleccion(OpenApiDocument documento) => new()
    {
        Type = JsonSchemaType.Object,
        Required = new HashSet<string> { "type", "generadoUtc", "features" },
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["type"] = Constante("FeatureCollection"),
            ["generadoUtc"] = Fecha("Hora del servidor; próximo `updatedSince`."),
            ["features"] = new OpenApiSchema
            {
                Type = JsonSchemaType.Array,
                Items = new OpenApiSchema
                {
                    OneOf = [new OpenApiSchemaReference("AnomaliaFeature", documento), new OpenApiSchemaReference("BajaFeature", documento)],
                },
            },
        },
    };

    internal static OpenApiSchema Anomalia() => Feature(Punto(), new OpenApiSchema
    {
        Type = JsonSchemaType.Object,
        Required = new HashSet<string> { "id", "kmVia", "tipo", "severidad", "persistencia", "ultimaDeteccionUtc", "venceUtc" },
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["id"] = Uuid(),
            ["kmVia"] = new OpenApiSchema { Type = JsonSchemaType.Number, Minimum = "0", Description = "Km sobre la vía (D13)." },
            ["tipo"] = Tipos(),
            ["severidad"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Minimum = "1", Maximum = "5" },
            ["persistencia"] = Catalogo("Episodica", "Persistente"),
            ["ultimaDeteccionUtc"] = Fecha(null),
            ["venceUtc"] = new OpenApiSchema
            {
                Type = JsonSchemaType.String | JsonSchemaType.Null, Format = "date-time",
                Description = "`null` en los estados finales (`Expirada`, `Reparada`, `Descartada`).",
            },
            ["estado"] = Estados(),
        },
    });

    /// <summary>Anomalía que dejó de ser visible para el rol desde `updatedSince` (RF-14); no revela su estado.</summary>
    internal static OpenApiSchema Baja() => Feature(new OpenApiSchema { Type = JsonSchemaType.Null }, new OpenApiSchema
    {
        Type = JsonSchemaType.Object,
        Required = new HashSet<string> { "id", "eliminada" },
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["id"] = Uuid(),
            ["eliminada"] = new OpenApiSchema { Type = JsonSchemaType.Boolean, Enum = [JsonValue.Create(true)] },
        },
    });

    private static OpenApiSchema Feature(OpenApiSchema geometria, OpenApiSchema propiedades) => new()
    {
        Type = JsonSchemaType.Object,
        Required = new HashSet<string> { "type", "id", "geometry", "properties" },
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["type"] = Constante("Feature"), ["id"] = Uuid(), ["geometry"] = geometria, ["properties"] = propiedades,
        },
    };

    private static OpenApiSchema Punto() => new()
    {
        Type = JsonSchemaType.Object,
        Required = new HashSet<string> { "type", "coordinates" },
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["type"] = Constante("Point"),
            ["coordinates"] = new OpenApiSchema
            {
                Type = JsonSchemaType.Array, MinItems = 2, MaxItems = 2, Description = "[longitud, latitud]",
                Items = new OpenApiSchema { Type = JsonSchemaType.Number },
            },
        },
    };

    private static OpenApiSchema Uuid() => new() { Type = JsonSchemaType.String, Format = "uuid" };

    private static OpenApiSchema Fecha(string? descripcion) =>
        new() { Type = JsonSchemaType.String, Format = "date-time", Description = descripcion };

    private static OpenApiSchema Constante(string valor) => new() { Type = JsonSchemaType.String, Const = valor };
}
