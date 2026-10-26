using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
namespace KillaUru.Anomalias.Adapters.Documentacion;
/// <summary>Explicita catálogos y campos requeridos del contrato, sin tocar otros módulos.</summary>
internal sealed class EsquemasAnomalias : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken ct)
    {
        var tipo = context.JsonTypeInfo.Type;
        if (tipo.Namespace?.StartsWith("KillaUru.Anomalias.Application", StringComparison.Ordinal) != true || schema.Properties is null) return Task.CompletedTask;
        foreach (var (nombre, propiedad) in schema.Properties.ToArray()) Completar(schema, nombre, propiedad);
        if (tipo.Name == "SolicitudExportacion") schema.Properties["tipo"] = Opcional(Catalogo("Bache", "Hundimiento"));
        schema.Required = tipo.Name switch
        {
            "SolicitudExportacion" => new HashSet<string> { "formato" }, "ComandoModeracion" => new HashSet<string> { "accion" },
            "EstadoHistorico" => new HashSet<string> { "hacia", "ocurridaUtc" },
            "TransicionReciente" => new HashSet<string> { "anomaliaId", "tipo", "kmVia", "hacia", "ocurridaUtc" },
            _ => schema.Properties.Keys.ToHashSet(),
        };
        if (tipo.Name == "FilaExportacion") schema.AdditionalPropertiesAllowed = false;
        return Task.CompletedTask;
    }
    private static void Completar(OpenApiSchema schema, string nombre, IOpenApiSchema propiedad)
    {
        if (nombre == "tipo") schema.Properties![nombre] = Catalogo("Bache", "Hundimiento");
        if (nombre is "estado" or "hacia") schema.Properties![nombre] = Estados();
        if (nombre == "desde") schema.Properties![nombre] = Opcional(Estados());
        if (nombre == "estados") schema.Properties![nombre] = new OpenApiSchema { Type = JsonSchemaType.Array, Items = Estados() };
        if (nombre == "persistencia") schema.Properties![nombre] = Catalogo("Episodica", "Persistente");
        if (nombre == "formato") schema.Properties![nombre] = Catalogo("CSV", "GeoJSON");
        if (propiedad is not OpenApiSchema valor) return;
        if ((valor.Type & (JsonSchemaType.Integer | JsonSchemaType.Number)) != 0) valor.Type &= ~JsonSchemaType.String;
        if (nombre is "severidad" or "severidadMostrada") { valor.Minimum = "1"; valor.Maximum = "5"; }
    }
    private static OpenApiSchema Estados() => Catalogo("Activa", "PendienteConfirmacion", "Expirada", "Reparada", "Descartada");
    private static OpenApiSchema Opcional(OpenApiSchema valor) => new()
    { OneOf = [valor, new OpenApiSchema { Type = JsonSchemaType.Null }] };
    private static OpenApiSchema Catalogo(params string[] valores) => new()
    { Type = JsonSchemaType.String, Enum = valores.Select(v => (JsonNode)JsonValue.Create(v)!).ToList() };
}
