using System.Text.Json.Nodes;
using KillaUru.Auditoria.Adapters.Dtos;
using KillaUru.Auditoria.Application;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
namespace KillaUru.Auditoria.Adapters.Documentacion;

/// <summary>Completa las restricciones del contrato que las propiedades string no expresan por sí solas.</summary>
internal sealed class EsquemasAuditoria : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type != typeof(RegistroAuditoriaDto)) return Task.CompletedTask;
        schema.Properties!["categoria"] = Enumeracion(FiltrosAuditoria.CategoriasPermitidas);
        schema.Properties["accion"] = Enumeracion(FiltrosAuditoria.AccionesPermitidas);
        schema.Properties["actorRol"] = Enumeracion(["ConductorRegistrado", "Administrador", "Sistema"]);
        schema.Properties["contexto"] = new OpenApiSchema { Type = JsonSchemaType.Object, AdditionalPropertiesAllowed = true };
        schema.Properties["objetivo"] = new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, MaxLength = 100 };
        return Task.CompletedTask;
    }

    internal static OpenApiSchema Enumeracion(IEnumerable<string> valores) => new()
    {
        Type = JsonSchemaType.String,
        Enum = valores.Select(v => (JsonNode)JsonValue.Create(v)!).ToList(),
    };
}
