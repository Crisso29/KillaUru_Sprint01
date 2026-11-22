using KillaUru.Ingesta.Adapters.Dtos;
using KillaUru.Ingesta.Application;
using KillaUru.Ingesta.Application.Recepcion;
using KillaUru.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace KillaUru.Ingesta.Adapters;

/// <summary>
/// POST /api/v1/anomalies/batch — recibe lotes de la app, construye el LoteEntrante y mapea el resultado.
/// Accesible para anónimos y ConductorRegistrado; el Administrador recibe 403 (FR-001; R-004).
/// Los errores son ProblemDetails con el `codigo` del contrato 005 (`lote_invalido`, `lote_demasiado_grande`, `saturado`).
/// </summary>
[ApiController]
[Route("api/v1/anomalies")]
internal sealed class SyncController(RecibirLoteEventosUseCase useCase, IOptions<IngestaOptions> opciones) : ControllerBase
{
    private const string BaseTipos = "https://api.killauru.pe/errores/";

    /// <summary>
    /// Recibe un lote de hasta 50 eventos, guarda los aceptados y devuelve el resultado por evento.
    /// </summary>
    /// <response code="200">Lote procesado; cada evento en una sola lista.</response>
    /// <response code="400">El cuerpo no era un lote válido (vacío, mal JSON, más de 50 eventos).</response>
    /// <response code="401">Token inválido o vencido.</response>
    /// <response code="403">El administrador no puede enviar detecciones.</response>
    /// <response code="413">Cuerpo de más de 256 KB (lote_demasiado_grande).</response>
    /// <response code="503">Cola saturada; reintentar tras Retry-After segundos.</response>
    [HttpPost("batch", Name = Documentacion.OperacionIngesta.OperationId)]
    [AllowAnonymous]
    [Consumes(typeof(LoteDto), "application/json")]
    [ProducesResponseType(typeof(RespuestaLoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RecibirLoteAsync(
        [FromHeader(Name = "X-KillaUru-Seudonimo")] string? seudonimoHeader,
        CancellationToken ct)
    {
        // Administrador no puede enviar detecciones (R-004)
        if (User.IsInRole(Roles.Administrador)) return Forbid();

        // El cuerpo se lee aquí, con el tope de la configuración, y no con [FromBody] (ver LectorCuerpoLote)
        var tope = opciones.Value.TamanoMaximoCuerpoKb * 1024L;
        var lectura = await LectorCuerpoLote.LeerAsync(Request.Body, Request.ContentLength, tope, ct);
        if (lectura.Estado == EstadoLectura.DemasiadoGrande) return Problema(StatusCodes.Status413PayloadTooLarge, "lote_demasiado_grande");
        if (lectura.Eventos is not { } eventos) return Problema(StatusCodes.Status400BadRequest, "lote_invalido");

        Guid? seudonimoCabecera = Guid.TryParse(seudonimoHeader, out var g) ? g : null;
        var resultado = await useCase.ProcesarAsync(new LoteEntrante(eventos, ConSesion(), seudonimoCabecera), ct);
        return Responder(resultado);
    }

    /// <summary>`con_sesion` = conductor registrado con el claim `verificado = true` (RN-14, D8).</summary>
    private bool ConSesion() =>
        User.IsInRole(Roles.ConductorRegistrado) && User.FindFirst("verificado")?.Value == "true";

    private IActionResult Responder(ResultadoLote resultado) => resultado switch
    {
        ResultadoLoteInvalido => Problema(StatusCodes.Status400BadRequest, "lote_invalido"),
        ResultadoLoteSaturado s => Saturado(s),
        ResultadoLoteAceptado a => Ok(CrearRespuesta(a)),
        _ => StatusCode(StatusCodes.Status500InternalServerError),
    };

    private static RespuestaLoteDto CrearRespuesta(ResultadoLoteAceptado a) => new()
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
    };

    private ObjectResult Saturado(ResultadoLoteSaturado s)
    {
        Response.Headers.RetryAfter = ((int)s.ReintentarEn.TotalSeconds).ToString();
        return Problema(StatusCodes.Status503ServiceUnavailable, "saturado");
    }

    /// <summary>ProblemDetails (RFC 9457) con `codigo`, `type` y `correlacionId`, sin datos internos (convenciones-api §3).</summary>
    private ObjectResult Problema(int estado, string codigo)
    {
        var problema = new ProblemDetails { Status = estado, Title = "No se pudo recibir el lote.", Type = BaseTipos + codigo };
        problema.Extensions["codigo"] = codigo;
        problema.Extensions["correlacionId"] = HttpContext.TraceIdentifier;
        return new ObjectResult(problema) { StatusCode = estado, ContentTypes = { "application/problem+json" } };
    }
}
