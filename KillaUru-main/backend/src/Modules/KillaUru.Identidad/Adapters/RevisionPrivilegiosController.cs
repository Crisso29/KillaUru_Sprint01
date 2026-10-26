using KillaUru.Identidad.Adapters.Dtos;
using KillaUru.Identidad.Application.Privilegios;
using KillaUru.Identidad.Application.Resultados;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace KillaUru.Identidad.Adapters;

/// <summary>Revisión completa de privilegios; el servidor calcula el próximo vencimiento.</summary>
[Route("api/v1/usuarios/revision-privilegios"), Authorize(Roles = "Administrador")]
[ProducesResponseType(typeof(ProblemDetails), 401), ProducesResponseType(typeof(ProblemDetails), 403)]
internal sealed class RevisionPrivilegiosController(ConsultarRevisionUseCase consultar, RegistrarRevisionUseCase registrar) : IdentidadControllerBase
{
    /// <summary>Consultar administradores y vencimiento de la revisión.</summary>
    /// <remarks>Rol: administrador. La lectura de correos también queda auditada.</remarks>
    [HttpGet(Name = "consultarRevisionPrivilegios"), ProducesResponseType(typeof(EstadoRevision), 200)]
    public async Task<IActionResult> Consultar(CancellationToken ct) => Ok(await consultar.EjecutarAsync(Actor, ct));

    /// <summary>Confirmar o retirar a cada administrador actual.</summary>
    /// <remarks>Rol: administrador. La lista debe ser completa, sin repeticiones ni intersecciones.</remarks>
    [HttpPost(Name = "registrarRevisionPrivilegios"), ProducesResponseType(typeof(EstadoRevision), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400), ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Registrar(RevisionSolicitud solicitud, CancellationToken ct)
    {
        var resultado = await registrar.EjecutarAsync(solicitud.Confirmados, solicitud.Retirados, Actor, ct);
        if (resultado.Error is not null) return RespuestaIdentidad.Error(resultado.Error, resultado.Estado, HttpContext);
        return StatusCode(201, resultado.Revision);
    }
}
