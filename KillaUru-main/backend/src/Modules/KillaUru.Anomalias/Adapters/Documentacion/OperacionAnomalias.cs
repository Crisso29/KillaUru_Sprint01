using KillaUru.Anomalias.Application.Exportacion;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
namespace KillaUru.Anomalias.Adapters.Documentacion;
/// <summary>Describe el stream de exportación con su esquema de fila compartido.</summary>
internal sealed class OperacionAnomalias : IOpenApiOperationTransformer
{
    public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken ct)
    {
        if (operation.OperationId != "exportarDataset") return;
        var fila = await context.GetOrCreateSchemaAsync(typeof(FilaExportacion), cancellationToken: ct);
        context.Document!.Components!.Schemas!["FilaExportacion"] = fila;
        operation.Responses!["200"] = new OpenApiResponse { Description = "Archivo sin seudónimos, auditado antes de escribir.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["text/csv"] = new() { Schema = new OpenApiSchema { Type = JsonSchemaType.String } },
                ["application/geo+json"] = new() { Schema = Coleccion(context.Document) },
            } };
    }
    private static OpenApiSchema Coleccion(OpenApiDocument document) => new()
    {
        Type = JsonSchemaType.Object, Required = new HashSet<string> { "type", "features" },
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["type"] = new OpenApiSchema { Type = JsonSchemaType.String, Const = "FeatureCollection" },
            ["features"] = new OpenApiSchema { Type = JsonSchemaType.Array, Items = new OpenApiSchema
            { Type = JsonSchemaType.Object, Required = new HashSet<string> { "type", "geometry", "properties" },
                Properties = new Dictionary<string, IOpenApiSchema> { ["type"] = new OpenApiSchema { Type = JsonSchemaType.String, Const = "Feature" },
                    ["geometry"] = new OpenApiSchema { Type = JsonSchemaType.Object }, ["properties"] = new OpenApiSchemaReference("FilaExportacion", document) } } },
        },
    };
}
