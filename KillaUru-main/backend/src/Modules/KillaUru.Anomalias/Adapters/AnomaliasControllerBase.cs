using KillaUru.Anomalias.Application.Administracion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace KillaUru.Anomalias.Adapters;
/// <summary>Seguridad común de administración; el actor siempre viene del token validado.</summary>
[ApiController, Authorize(Roles = "Administrador"), EnableRateLimiting("consultas")]
[ProducesResponseType(typeof(ProblemDetails), 401), ProducesResponseType(typeof(ProblemDetails), 403)]
[ProducesResponseType(typeof(ProblemDetails), 429)]
internal abstract class AnomaliasControllerBase : ControllerBase
{
    protected ActorAnomalias Actor => new(Guid.Parse(User.FindFirst("sub")!.Value), User.FindFirst("rol")!.Value);
    /// <summary>ProblemDetails con el código estable del contrato y el id de correlación para buscar el log.</summary>
    protected ObjectResult Error(string codigo, int estado)
    {
        var problema = new ProblemDetails { Status = estado, Title = "No se pudo completar la operación.", Type = "https://api.killauru.pe/errores/" + codigo };
        problema.Extensions["codigo"] = codigo;
        problema.Extensions["correlacionId"] = HttpContext.TraceIdentifier;
        return new(problema) { StatusCode = estado, ContentTypes = { "application/problem+json" } };
    }
}
