using Microsoft.OpenApi;
namespace KillaUru.Api.Documentacion;

/// <summary>Describe errores comunes con su código y correlación, sin filtrar detalles internos.</summary>
internal static class RespuestasComunes
{
    internal static void Completar(OpenApiOperation operacion)
    {
        foreach (var (codigo, respuesta) in operacion.Responses!)
        {
            if (!int.TryParse(codigo, out var estado) || estado < 400 || respuesta is not OpenApiResponse editable) continue;
            editable.Content = new Dictionary<string, OpenApiMediaType>
                { ["application/problem+json"] = new() { Schema = Problema() } };
            if (estado is 429 or 503)
            {
                editable.Headers = new Dictionary<string, IOpenApiHeader>
                { ["Retry-After"] = new OpenApiHeader { Description = "Segundos antes de reintentar.", Schema = new OpenApiSchema { Type = JsonSchemaType.Integer } } };
            }
        }
    }

    private static OpenApiSchema Problema() => new()
    {
        Type = JsonSchemaType.Object,
        Required = new HashSet<string> { "type", "title", "status", "codigo", "correlacionId" },
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["type"] = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uri" },
            ["title"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer },
            ["codigo"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["correlacionId"] = new OpenApiSchema { Type = JsonSchemaType.String },
        },
    };
}
