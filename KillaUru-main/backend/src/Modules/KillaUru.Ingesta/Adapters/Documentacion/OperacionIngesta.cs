using KillaUru.Ingesta.Adapters.Dtos;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace KillaUru.Ingesta.Adapters.Documentacion;

/// <summary>
/// Declara el cuerpo de `recibirLote` con el esquema `Lote`. Hace falta porque el controlador lee el cuerpo con
/// <see cref="LectorCuerpoLote"/> (tope configurable) en vez de `[FromBody]`, y entonces MVC no lo describe solo.
/// </summary>
internal sealed class OperacionIngesta : IOpenApiOperationTransformer
{
    internal const string OperationId = "recibirLote";

    public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken ct)
    {
        if (operation.OperationId != OperationId) return;
        var documento = context.Document!;
        documento.Components!.Schemas!["Lote"] = await context.GetOrCreateSchemaAsync(typeof(LoteDto), cancellationToken: ct);
        operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new() { Schema = new OpenApiSchemaReference("Lote", documento) },
            },
        };
    }
}
