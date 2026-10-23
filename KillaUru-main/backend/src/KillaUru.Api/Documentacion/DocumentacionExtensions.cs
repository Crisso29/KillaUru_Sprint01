using KillaUru.Api.Configuracion;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

namespace KillaUru.Api.Documentacion;

/// <summary>
/// Documentación viva (constitución IX): el OpenAPI 3.1 se genera desde el código y Scalar lo muestra
/// en /docs cuando <c>DocumentacionPublica</c> está activada. El valor por defecto de producción es false.
/// </summary>
/// <remarks>
/// Production impide publicar la documentación incluso si una variable activa la opción (FR-015).
/// </remarks>
internal static class DocumentacionExtensions
{
    internal static IServiceCollection AgregarDocumentacion(this IServiceCollection services)
    {
        services.AddOpenApi("v1", opciones =>
        {
            opciones.CreateSchemaReferenceId = info => info.Type.Name.EndsWith("Dto", StringComparison.Ordinal)
                ? info.Type.Name[..^3] : Microsoft.AspNetCore.OpenApi.OpenApiOptions.CreateDefaultSchemaReferenceId(info);
            opciones.AddDocumentTransformer<EsquemasSeguridad>();
            opciones.AddDocumentTransformer((documento, _, _) =>
            {
                documento.Info.Title = "KillaUru API";
                documento.Info.Description = "Detección de anomalías viales en la PE-28A. Errores en ProblemDetails (RFC 9457).";
                return Task.CompletedTask;
            });
            opciones.AddOperationTransformer<TransformadorSeguridad>();
        });
        return services;
    }

    internal static WebApplication MapearDocumentacion(this WebApplication app)
    {
        if (!app.Environment.IsProduction() && app.Services.GetRequiredService<IOptions<ApiOptions>>().Value.DocumentacionPublica)
        {
            app.MapOpenApi("/openapi/{documentName}.json");
            app.MapScalarApiReference("/docs", opciones => opciones.WithTitle("KillaUru API"));
        }

        return app;
    }
}
