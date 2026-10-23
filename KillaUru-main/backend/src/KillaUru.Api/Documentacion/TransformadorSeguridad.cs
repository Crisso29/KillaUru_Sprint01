using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace KillaUru.Api.Documentacion;

/// <summary>
/// Completa cada operación de /api/v1 con lo que exige convenciones-api.md §4: la línea «Rol: …» y las
/// respuestas comunes 401/403 (si pide sesión) y 429 (todas). Así ningún endpoint olvida documentarlas.
/// </summary>
/// <remarks>
/// Documenta credenciales alternativas, errores ProblemDetails y Retry-After en las respuestas de límite.
/// </remarks>
internal sealed class TransformadorSeguridad : IOpenApiOperationTransformer
{
    private const string PrefijoNegocio = "api/v1";

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var ruta = context.Description.RelativePath ?? string.Empty;
        if (!ruta.StartsWith(PrefijoNegocio, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var metadatos = context.Description.ActionDescriptor.EndpointMetadata;
        var autorizacion = metadatos.OfType<IAuthorizeData>().ToList();
        var esPublico = autorizacion.Count == 0 || metadatos.OfType<IAllowAnonymous>().Any();
        var roles = string.Join(", ", autorizacion.Select(a => a.Roles).Where(r => !string.IsNullOrEmpty(r)));

        operation.Description = $"Rol: {(esPublico ? "público" : roles.Length > 0 ? roles : "con sesión")}. {operation.Description}".Trim();
        operation.Responses ??= [];
        Agregar(operation, "429", "Límite de tasa superado (`limite_excedido`), con `Retry-After`.");
        if (!esPublico)
        {
            operation.Security =
            [
                new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("tokenAcceso", context.Document)] = [] },
                new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("cookieAcceso", context.Document)] = [] },
            ];
            Agregar(operation, "401", "Token inválido, vencido o ausente (`token_invalido`).");
            Agregar(operation, "403", "El rol no tiene permiso (`sin_permiso`).");
        }

        RespuestasComunes.Completar(operation);
        return Task.CompletedTask;
    }

    private static void Agregar(OpenApiOperation operacion, string codigo, string descripcion) =>
        operacion.Responses!.TryAdd(codigo, new OpenApiResponse { Description = descripcion });
}
