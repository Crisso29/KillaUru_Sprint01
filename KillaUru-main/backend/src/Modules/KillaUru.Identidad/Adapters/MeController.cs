using KillaUru.Identidad.Adapters.Dtos;
using KillaUru.Identidad.Application.Cuenta;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Application.Titular;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace KillaUru.Identidad.Adapters;

/// <summary>Derechos del titular aplicados exclusivamente a la cuenta autenticada.</summary>
[Route("api/v1/me"), Authorize]
internal sealed class MeController(ObtenerMisDatosUseCase obtener, EliminarMiCuentaUseCase eliminar,
    AceptarVersionUseCase aceptar, ReenviarVerificacionUseCase reenviar) : IdentidadControllerBase
{
    /// <summary>Ver mis datos y mis aceptaciones.</summary>
    /// <remarks>Rol: conductor o administrador.</remarks>
    [HttpGet(Name = "obtenerMisDatos"), ProducesResponseType(typeof(MisDatos), 200), ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> Obtener(CancellationToken ct)
    {
        var datos = await obtener.EjecutarAsync(UsuarioId, ct);
        return datos is null ? RespuestaIdentidad.Error("token_invalido", 401, HttpContext) : Ok(datos);
    }

    /// <summary>Eliminar definitivamente mi cuenta confirmando la contraseña.</summary>
    /// <remarks>Rol: conductor. Elimina datos personales y credenciales; conserva auditoría sin correo.</remarks>
    [Authorize(Roles = "ConductorRegistrado"), HttpDelete(Name = "eliminarMiCuenta")]
    [ProducesResponseType(204), ProducesResponseType(typeof(ProblemDetails), 401), ProducesResponseType(typeof(ProblemDetails), 403)]
    public async Task<IActionResult> Eliminar(EliminarCuentaSolicitud solicitud, CancellationToken ct) =>
        RespuestaIdentidad.Gestion(await eliminar.EjecutarAsync(UsuarioId, solicitud.Contrasena, ct), HttpContext);

    /// <summary>Aceptar una nueva versión de un documento legal.</summary>
    /// <remarks>Rol: conductor. Solo se acepta la versión vigente.</remarks>
    [Authorize(Roles = "ConductorRegistrado"), HttpPost("aceptaciones", Name = "aceptarVersion")]
    [ProducesResponseType(204), ProducesResponseType(typeof(ProblemDetails), 400), ProducesResponseType(typeof(ProblemDetails), 401), ProducesResponseType(typeof(ProblemDetails), 403)]
    public async Task<IActionResult> Aceptar(AceptacionSolicitud solicitud, CancellationToken ct) =>
        RespuestaIdentidad.Gestion(await aceptar.EjecutarAsync(UsuarioId, solicitud.Documento, solicitud.Version, ct), HttpContext);

    /// <summary>Reenviar mi enlace de verificación.</summary>
    /// <remarks>Rol: conductor o administrador. Invalida la verificación pendiente.</remarks>
    [HttpPost("reenviar-verificacion", Name = "reenviarMiVerificacion")]
    [ProducesResponseType(202), ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> Reenviar(CancellationToken ct) =>
        RespuestaIdentidad.Gestion(await reenviar.EjecutarAsync(UsuarioId, ct), HttpContext);
}
