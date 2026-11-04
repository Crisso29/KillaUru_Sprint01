using KillaUru.Dashboard.Adapters.Dtos;
using KillaUru.Dashboard.Application;
using KillaUru.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KillaUru.Dashboard.Adapters;

/// <summary>Expone los indicadores del panel operativo. Solo para administradores (FR-013).</summary>
[ApiController]
[Route("api/v1/dashboard")]
[Authorize(Roles = Roles.Administrador)]
internal sealed class DashboardController(ConsultarResumenUseCase consultar) : ControllerBase
{
    /// <summary>Devuelve el resumen de indicadores del dashboard para el periodo indicado.</summary>
    /// <param name="dias">Días del periodo: 7, 14 o 30.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <response code="200">Resumen de indicadores.</response>
    /// <response code="400">Periodo no permitido (solo 7, 14 o 30).</response>
    /// <response code="401">Token de acceso ausente o inválido.</response>
    /// <response code="403">El token no pertenece a un administrador.</response>
    [HttpGet("indicadores")]
    [ProducesResponseType<ResumenDashboardDto>(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> ObtenerResumen([FromQuery] int dias = 7, CancellationToken ct = default)
    {
        if (dias is not (7 or 14 or 30)) return BadRequest();
        var resumen = await consultar.EjecutarAsync(dias, ct);
        return Ok(ResumenDashboardDto.Desde(resumen));
    }
}
