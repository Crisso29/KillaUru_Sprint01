using KillaUru.Ingesta.Adapters.Dtos;
using KillaUru.Ingesta.Application.Recepcion;
using KillaUru.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KillaUru.Ingesta.Adapters;

/// <summary>
/// POST /api/v1/anomalies/batch — recibe lotes de la app, construye el LoteEntrante y mapea el resultado.
/// Accesible para anónimos y ConductorRegistrado; el Administrador recibe 403 (FR-001; R-004).
/// </summary>
[ApiController]
[Route("api/v1/anomalies")]
internal sealed class SyncController(RecibirLoteEventosUseCase useCase) : ControllerBase
{
    /// <summary>
    /// Recibe un lote de hasta 50 eventos, guarda los aceptados y devuelve el resultado por evento.
    /// </summary>
    /// <response code="200">Lote procesado; cada evento en una sola lista.</response>
    /// <response code="400">El cuerpo no era un lote válido (vacío, mal JSON, más de 50 eventos).</response>
    /// <response code="401">Token inválido o vencido.</response>
    /// <response code="403">El administrador no puede enviar detecciones.</response>
    /// <response code="503">Cola saturada; reintentar tras Retry-After segundos.</response>
    [HttpPost("batch")]
    [AllowAnonymous]
    [RequestSizeLimit(256 * 1024)]
    [ProducesResponseType(typeof(RespuestaLoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RecibirLoteAsync(
        [FromBody] LoteDto? dto,
        [FromHeader(Name = "X-KillaUru-Seudonimo")] string? seudonimoHeader,
        CancellationToken ct)
    {
        // Administrador no puede enviar detecciones (R-004)
        if (User.IsInRole(Roles.Administrador)) return Forbid();

        if (dto?.Eventos == null) return BadRequest(new { codigo = "lote_invalido" });

        var conSesion = User.IsInRole(Roles.ConductorRegistrado)
            && User.FindFirst("verificado")?.Value == "true";

        Guid? seudonimoCabecera = Guid.TryParse(seudonimoHeader, out var g) ? g : null;

        var lote = new LoteEntrante(dto.Eventos, conSesion, seudonimoCabecera);
        var resultado = await useCase.ProcesarAsync(lote, ct);

        return resultado switch
        {
            ResultadoLote.Invalido => BadRequest(new { codigo = "lote_invalido" }),
            ResultadoLote.Saturado s => RetryAfter(s),
            ResultadoLote.Aceptado a => Ok(new RespuestaLoteDto
            {
                LoteId = a.LoteId,
                Aceptados = a.Aceptados,
                Duplicados = a.Duplicados,
                Rechazados = a.Rechazados.Select(r => new RechazoDto
                {
                    Id = r.Id,
                    Indice = r.Indice,
                    Motivo = r.Motivo.ToString(),
                }).ToList(),
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    private ObjectResult RetryAfter(ResultadoLote.Saturado s)
    {
        Response.Headers["Retry-After"] = ((int)s.ReintentarEn.TotalSeconds).ToString();
        return StatusCode(StatusCodes.Status503ServiceUnavailable, new { codigo = "saturado" });
    }
}
