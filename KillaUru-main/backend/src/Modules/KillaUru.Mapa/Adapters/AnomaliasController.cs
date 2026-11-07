using KillaUru.Mapa.Adapters.Dtos;
using KillaUru.Mapa.Application;
using KillaUru.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace KillaUru.Mapa.Adapters;

/// <summary>
/// GET /api/v1/anomalies — mapa GeoJSON con recorte por rol, caché, ETag y filtros.
/// FR-001; ADR-006; spec 006.
/// </summary>
[ApiController, Route("api/v1"), AllowAnonymous, EnableRateLimiting("consultas")]
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 403)]
[ProducesResponseType(typeof(ProblemDetails), 429)]
internal sealed class AnomaliasController(
    ConsultarMapaUseCase useCase,
    IOptions<MapaOptions> opciones,
    TimeProvider reloj) : ControllerBase
{
    private const string MimeGeoJson = "application/geo+json";

    [HttpGet("anomalies")]
    [Produces(MimeGeoJson)]
    [ProducesResponseType(200), ProducesResponseType(304)]
    public async Task<IActionResult> ObtenerMapa([FromQuery] ParametrosMapaDto dto, CancellationToken ct)
    {
        var error = ValidarParametros(dto);
        if (error is not null) return error;
        var consulta = ConsultaMapa.Crear(ObtenerRol(), dto.Bbox, dto.Tipo, dto.Severidades,
            dto.Desde, dto.Hasta, dto.Estados, dto.UpdatedSince,
            Request.Headers.IfNoneMatch.FirstOrDefault());
        if (consulta is null) return Problema("datos_invalidos", 400);
        return Responder(await useCase.EjecutarAsync(consulta, ct));
    }

    private ObjectResult? ValidarParametros(ParametrosMapaDto dto)
    {
        bool hayFiltros = dto.Tipo.HasValue || (dto.Severidades is { Length: > 0 })
                          || dto.Desde.HasValue || dto.Hasta.HasValue;

        if (dto.UpdatedSince.HasValue && hayFiltros)
            return Problema("parametros_incompatibles", 400);

        if (dto.UpdatedSince.HasValue && dto.UpdatedSince.Value > reloj.GetUtcNow().AddMinutes(5))
            return Problema("datos_invalidos", 400);

        return null;
    }

    private IActionResult Responder(ResultadoMapa resultado) => resultado switch
        {
            ResultadoMapaProhibido   => Problema("sin_permiso", 403),
            ResultadoMapaInvalido  i => Problema(i.Codigo, 400),
            ResultadoMapaNoModificado nm => RespuestaSinCuerpo(nm.ETag),
            ResultadoMapaContenido c => RespuestaConCuerpo(c),
            _ => StatusCode(500),
        };

    private string ObtenerRol()
    {
        if (User.Identity?.IsAuthenticated != true) return Roles.Anonimo;
        return User.FindFirst("rol")?.Value ?? Roles.Anonimo;
    }

    private FileContentResult RespuestaConCuerpo(ResultadoMapaContenido c)
    {
        if (c.ETag is not null) { Response.Headers.ETag = c.ETag; }
        Response.Headers.CacheControl = c.Publico
            ? $"public, max-age={opciones.Value.MaxAgeBordeSegundos}"
            : "private, no-store";
        return new FileContentResult(c.Cuerpo, MimeGeoJson);
    }

    private StatusCodeResult RespuestaSinCuerpo(string etag)
    {
        Response.Headers.ETag = etag;
        return StatusCode(304);
    }

    private ObjectResult Problema(string codigo, int estado)
    {
        var pd = new ProblemDetails
        {
            Status = estado,
            Title  = "No se pudo completar la operación.",
            Type   = $"https://api.killauru.pe/errores/{codigo}",
        };
        pd.Extensions["codigo"] = codigo;
        pd.Extensions["correlacionId"] = HttpContext.TraceIdentifier;
        return new ObjectResult(pd) { StatusCode = estado, ContentTypes = { "application/problem+json" } };
    }
}
