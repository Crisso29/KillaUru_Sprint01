using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
namespace KillaUru.Identidad.Adapters.Documentacion;

/// <summary>Restricciones del contrato que string y los resultados de Application no expresan por sí solos.</summary>
internal sealed class EsquemasIdentidad : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var tipo = context.JsonTypeInfo.Type;
        if (tipo.Namespace?.StartsWith("KillaUru.Identidad.", StringComparison.Ordinal) != true || schema.Properties is null) return Task.CompletedTask;
        foreach (var (nombre, propiedad) in schema.Properties.ToArray()) Completar(schema, nombre, propiedad);
        schema.Required = schema.Properties.Keys.Where(k => k is not ("ultimaSesionUtc" or "ultimaRevisionUtc" or "turnstileToken" or "tokenRenovacion")).ToHashSet();
        if (tipo.Name == "SesionEmitida") schema.Required.Add("tokenRenovacion");
        if (tipo.Name == "ConsultaUsuarios") schema.Required.Clear();
        return Task.CompletedTask;
    }

    private static void Completar(OpenApiSchema esquema, string nombre, IOpenApiSchema propiedad)
    {
        if (nombre == "rol") esquema.Properties![nombre] = Enum("ConductorRegistrado", "Administrador");
        if (nombre == "estado") esquema.Properties![nombre] = Enum("Activa", "Desactivada", "InvitacionPendiente");
        if (nombre == "documento") esquema.Properties![nombre] = Enum("PoliticaPrivacidad", "TerminosUso");
        if (nombre == "canal") esquema.Properties![nombre] = Enum("app", "panel");
        if (propiedad is not OpenApiSchema valor) return;
        if (nombre == "correo") valor.Format = "email";
        if (nombre is "contrasena" or "nuevaContrasena") { valor.MinLength = 10; valor.MaxLength = 128; }
        if (nombre is "pagina" or "tamano" or "total") valor.Type = JsonSchemaType.Integer;
        if (nombre is "versionPoliticaAceptada" or "versionTerminosAceptada" or "turnstileToken" or "tokenRenovacion")
            valor.Type = JsonSchemaType.String;
    }

    internal static OpenApiSchema Enum(params string[] valores) => new()
    {
        Type = JsonSchemaType.String, Enum = valores.Select(v => (JsonNode)JsonValue.Create(v)!).ToList(),
    };
}
