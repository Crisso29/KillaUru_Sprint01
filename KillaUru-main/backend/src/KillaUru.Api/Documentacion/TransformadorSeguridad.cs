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
        if (!(context.Description.RelativePath ?? string.Empty).StartsWith(PrefijoNegocio, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var metadatos = context.Description.ActionDescriptor.EndpointMetadata;
        var autorizacion = metadatos.OfType<IAuthorizeData>().ToList();
        var esPublico = autorizacion.Count == 0 || metadatos.OfType<IAllowAnonymous>().Any();
        DescribirRol(operation, autorizacion, esPublico);
        operation.Responses ??= [];
        Agregar(operation, "429", "Límite de tasa superado (`limite_excedido`), con `Retry-After`.");
        if (!esPublico)
        {
            AgregarCredenciales(operation, context.Document);
        }

        RespuestasComunes.Completar(operation);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Prefija la descripción del endpoint con "Rol: …" según su metadato de autorización.
    /// Permite al consumidor de la API ver de un vistazo quién puede llamar a cada operación.
    /// </summary>
    private static void DescribirRol(OpenApiOperation operacion, List<IAuthorizeData> autorizacion, bool publico)
    {
        if (publico)
        {
            operacion.Description = $"Rol: público. {operacion.Description}".Trim();
            return;
        }

        var esSoloAdmin = autorizacion.Any(a => a.Policy == "SoloAdministrador");
        var rolesExplicitos = string.Join(", ", autorizacion.Select(a => a.Roles).Where(r => !string.IsNullOrEmpty(r)));

        var etiquetaRol = esSoloAdmin          ? "Administrador"
                        : rolesExplicitos.Length > 0 ? rolesExplicitos
                        :                              "con sesión";

        operacion.Description = $"Rol: {etiquetaRol}. {operacion.Description}".Trim();
    }

    private static void AgregarCredenciales(OpenApiOperation operacion, OpenApiDocument? documento)
    {
        operacion.Security =
        [
            new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("tokenAcceso", documento)] = [] },
            new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("cookieAcceso", documento)] = [] },
        ];
        Agregar(operacion, "401", "Token inválido, vencido o ausente (`token_invalido`).");
        Agregar(operacion, "403", "El rol no tiene permiso (`sin_permiso`).");
    }

    private static void Agregar(OpenApiOperation operacion, string codigo, string descripcion) =>
        operacion.Responses!.TryAdd(codigo, new OpenApiResponse { Description = descripcion });
}
