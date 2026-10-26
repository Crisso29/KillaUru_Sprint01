using KillaUru.Identidad.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace KillaUru.Identidad.Adapters;

/// <summary>Identidad del llamador validada por el host; no acepta actores suministrados en el cuerpo.</summary>
[ApiController]
[EnableRateLimiting("consultas")]
[ProducesResponseType(typeof(ProblemDetails), 429)]
internal abstract class IdentidadControllerBase : ControllerBase
{
    protected Guid UsuarioId => Guid.Parse(User.FindFirst("sub")!.Value);
    protected ActorIdentidad Actor => new(UsuarioId, User.FindFirst("rol")!.Value);
}
