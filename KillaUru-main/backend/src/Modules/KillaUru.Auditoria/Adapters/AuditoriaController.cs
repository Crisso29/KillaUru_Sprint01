using KillaUru.Auditoria.Adapters.Dtos;
using KillaUru.Auditoria.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace KillaUru.Auditoria.Adapters;

/// <summary>Frontera HTTP de solo lectura; la aplicación valida y registra el acceso sensible.</summary>
[ApiController, Route("api/v1/auditoria"), Authorize(Policy = "SoloAdministrador"), EnableRateLimiting("consultas")]
internal sealed class AuditoriaController(ConsultarAuditoriaUseCase consultar) : ControllerBase
{
    /// <summary>Consultar el registro de auditoría.</summary>
    /// <remarks>Rol: Administrador. Filtra y pagina de más reciente a más antiguo; la consulta queda auditada.</remarks>
    /// <param name="filtros">Fechas inclusivas, categoría, acción y actor.</param>
    /// <param name="ct">Cancelación de la petición.</param>
    /// <returns>Página de registros inmutables o un error ProblemDetails.</returns>
    [HttpGet(Name = "consultarAuditoria")]
    [ProducesResponseType<PaginaAuditoriaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Consultar([FromQuery] ConsultaAuditoriaDto filtros, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var actor)) return Unauthorized();
        var resultado = await consultar.EjecutarAsync(filtros.AEntrada(), actor, User.FindFirst("rol")!.Value, ct);
        return resultado is null ? BadRequest() : Ok(PaginaAuditoriaDto.De(resultado));
    }
}
