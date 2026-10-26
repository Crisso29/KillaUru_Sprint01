using KillaUru.Identidad.Adapters.Dtos;
using KillaUru.Identidad.Application.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace KillaUru.Identidad.Adapters;

/// <summary>Cambios administrativos con revocación y auditoría atómicas.</summary>
[Route("api/v1/usuarios/{id:guid}"), Authorize(Roles = "Administrador")]
[ProducesResponseType(typeof(ProblemDetails), 401), ProducesResponseType(typeof(ProblemDetails), 403), ProducesResponseType(typeof(ProblemDetails), 404)]
internal sealed class EstadoUsuarioController(CambiarRolUseCase cambiar, DesactivarUsuarioUseCase desactivar,
    ActivarUsuarioUseCase activar, ReenviarCorreoUsuarioUseCase reenviar) : IdentidadControllerBase
{
    /// <summary>Cambiar rol y revocar sesiones.</summary>
    /// <remarks>Rol: administrador. No permite quitarse el rol ni retirar al último administrador.</remarks>
    [HttpPut("rol", Name = "cambiarRol"), ProducesResponseType(204), ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Cambiar(Guid id, CambiarRolSolicitud solicitud, CancellationToken ct) =>
        RespuestaIdentidad.Gestion(await cambiar.EjecutarAsync(id, solicitud.Rol, Actor, ct), HttpContext);

    /// <summary>Desactivar cuenta y revocar sesiones.</summary>
    /// <remarks>Rol: administrador. El motivo es obligatorio; no se persiste texto personal en auditoría.</remarks>
    [HttpPost("desactivar", Name = "desactivarUsuario"), ProducesResponseType(204), ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Desactivar(Guid id, DesactivarSolicitud solicitud, CancellationToken ct) =>
        RespuestaIdentidad.Gestion(await desactivar.EjecutarAsync(id, solicitud.Motivo, Actor, ct), HttpContext);

    /// <summary>Reactivar una cuenta.</summary>
    /// <remarks>Rol: administrador. Las sesiones revocadas permanecen inválidas.</remarks>
    [HttpPost("activar", Name = "activarUsuario"), ProducesResponseType(204)]
    public async Task<IActionResult> Activar(Guid id, CancellationToken ct) =>
        RespuestaIdentidad.Gestion(await activar.EjecutarAsync(id, Actor, ct), HttpContext);

    /// <summary>Reenviar la invitación o verificación pendiente.</summary>
    /// <remarks>Rol: administrador. Invalida el enlace anterior del mismo propósito.</remarks>
    [HttpPost("reenviar-correo", Name = "reenviarCorreoUsuario"), ProducesResponseType(202)]
    public async Task<IActionResult> Reenviar(Guid id, CancellationToken ct) =>
        RespuestaIdentidad.Gestion(await reenviar.EjecutarAsync(id, Actor, ct), HttpContext);
}
