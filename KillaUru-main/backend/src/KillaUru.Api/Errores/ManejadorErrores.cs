using Microsoft.AspNetCore.Diagnostics;

namespace KillaUru.Api.Errores;

/// <summary>
/// Última red: una excepción inesperada se registra completa en el log y el cliente recibe solo
/// un 500 <c>error_interno</c> con su correlacionId, sin trazas ni detalles internos (FR-012).
/// Los resultados de negocio (rechazos, descartes) nunca llegan aquí: viajan como Result.
/// </summary>
/// <param name="logger">Registro de la excepción.</param>
internal sealed class ManejadorErrores(ILogger<ManejadorErrores> logger) : IExceptionHandler
{
    private static readonly Action<ILogger, string, Exception?> ErrorNoControlado =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(1, nameof(ErrorNoControlado)), "Error no controlado ({CorrelacionId})");

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ErrorNoControlado(logger, httpContext.TraceIdentifier, exception);
        await RespuestaProblema.EscribirAsync(
            httpContext, StatusCodes.Status500InternalServerError, "error_interno", "Ocurrió un error inesperado.");
        return true;
    }
}
