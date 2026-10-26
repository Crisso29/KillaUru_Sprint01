using KillaUru.Identidad.Application.Resultados;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
namespace KillaUru.Identidad.Adapters;

/// <summary>Errores consistentes con el host sin reflejar credenciales ni valores de entrada.</summary>
internal static class RespuestaIdentidad
{
    internal static IActionResult Gestion(ResultadoGestion resultado, HttpContext contexto) =>
        resultado.Error is null ? new StatusCodeResult(resultado.Estado) : Error(resultado.Error, resultado.Estado, contexto);

    internal static ObjectResult Error(string codigo, int estado, HttpContext contexto, DateTimeOffset? bloqueo = null)
    {
        var problema = new ProblemDetails { Status = estado, Title = "No se pudo completar la operación.",
            Type = "https://api.killauru.pe/errores/" + codigo };
        problema.Extensions["codigo"] = codigo;
        problema.Extensions["correlacionId"] = contexto.TraceIdentifier;
        if (bloqueo is not null) problema.Extensions["bloqueadoHastaUtc"] = bloqueo;
        return new ObjectResult(problema) { StatusCode = estado, ContentTypes = { "application/problem+json" } };
    }
}
