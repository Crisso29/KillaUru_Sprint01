using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
namespace KillaUru.Api.Documentacion;

/// <summary>Declara las dos credenciales alternativas aceptadas por el mismo middleware JWT.</summary>
internal sealed class EsquemasSeguridad : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["tokenAcceso"] = new OpenApiSecurityScheme
            { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" };
        document.Components.SecuritySchemes["cookieAcceso"] = new OpenApiSecurityScheme
            { Type = SecuritySchemeType.ApiKey, In = ParameterLocation.Cookie, Name = "ku_acceso" };
        return Task.CompletedTask;
    }
}
