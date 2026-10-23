using Microsoft.AspNetCore.Mvc;

namespace KillaUru.Api.Errores;

/// <summary>
/// Completa los errores escritos mediante IProblemDetailsService con un <c>codigo</c> estable, su <c>type</c> y el
/// <c>correlacionId</c> de la petición (convenciones-api.md §3). Nunca agrega trazas ni datos internos.
/// </summary>
/// <remarks>
/// UseStatusCodePages cubre estados sin cuerpo. La validación MVC usa la misma convención;
/// omite valores de entrada y mensajes del binder para no reflejar datos personales.
/// </remarks>
internal static class ProblemDetailsExtensions
{
    private const string BaseTipos = "https://api.killauru.pe/errores/";

    internal static IServiceCollection AgregarProblemDetails(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(opciones => opciones.InvalidModelStateResponseFactory = contexto =>
            new BadRequestObjectResult(new ProblemDetails
            {
                Status = 400, Title = "Los filtros o datos enviados no son válidos.", Type = BaseTipos + "datos_invalidos",
                Extensions = { ["codigo"] = "datos_invalidos", ["correlacionId"] = contexto.HttpContext.TraceIdentifier },
            }) { ContentTypes = { "application/problem+json" } });
        return services.AddProblemDetails(opciones => opciones.CustomizeProblemDetails = contexto =>
        {
            var problema = contexto.ProblemDetails;
            var estado = problema.Status ?? contexto.HttpContext.Response.StatusCode;
            var codigo = problema.Extensions.TryGetValue("codigo", out var valor) && valor is string texto
                ? texto
                : CodigoPorEstado(estado);
            problema.Extensions["codigo"] = codigo;
            problema.Extensions["correlacionId"] = contexto.HttpContext.TraceIdentifier;
            problema.Type = BaseTipos + codigo;
            problema.Extensions.Remove("traceId");
        });
    }

    internal static string CodigoPorEstado(int estado) => estado switch
    {
        StatusCodes.Status400BadRequest => "datos_invalidos",
        StatusCodes.Status401Unauthorized => "token_invalido",
        StatusCodes.Status403Forbidden => "sin_permiso",
        StatusCodes.Status404NotFound => "no_encontrado",
        StatusCodes.Status405MethodNotAllowed => "metodo_no_permitido",
        StatusCodes.Status409Conflict => "conflicto",
        StatusCodes.Status429TooManyRequests => "limite_excedido",
        StatusCodes.Status503ServiceUnavailable => "saturado",
        _ => "error_interno",
    };
}
