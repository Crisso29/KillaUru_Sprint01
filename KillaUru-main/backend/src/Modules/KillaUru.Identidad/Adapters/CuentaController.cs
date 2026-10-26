using KillaUru.Identidad.Adapters.Dtos;
using KillaUru.Identidad.Application.Cuenta;
using KillaUru.Identidad.Application.Resultados;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace KillaUru.Identidad.Adapters;

/// <summary>Registro y enlaces de correo; ninguna acción acepta roles ni ids de cuenta.</summary>
[Route("api/v1/auth")]
[AllowAnonymous]
internal sealed class CuentaController(RegistrarConductorUseCase registrar, VerificarCorreoUseCase verificar,
    SolicitarRecuperacionUseCase recuperar, FijarContrasenaConEnlaceUseCase fijar) : IdentidadControllerBase
{
    /// <summary>Crear cuenta de conductor con consentimiento.</summary>
    /// <remarks>Rol: público. Retorna sesión sin correo verificado. Máximo cinco registros por IP y hora.</remarks>
    [HttpPost("registro", Name = "registrarConductor"), EnableRateLimiting("registro")]
    [ProducesResponseType(typeof(SesionEmitida), 201), ProducesResponseType(typeof(ProblemDetails), 400), ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Registrar(RegistroSolicitud solicitud, CancellationToken ct)
    {
        var entrada = new RegistroEntrada(solicitud.Correo, solicitud.Contrasena,
            solicitud.VersionPoliticaAceptada, solicitud.VersionTerminosAceptada);
        var resultado = await registrar.EjecutarAsync(entrada, ct);
        if (resultado.Error is not null) return RespuestaIdentidad.Error(resultado.Error, resultado.Estado, HttpContext);
        return StatusCode(201, resultado.Sesion);
    }

    /// <summary>Verificar correo con un enlace de un solo uso.</summary>
    /// <remarks>Rol: público. El token se obtiene del fragmento del enlace de verificación.</remarks>
    [HttpPost("verificar-correo", Name = "verificarCorreo")]
    [ProducesResponseType(204), ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Verificar(TokenEnlace solicitud, CancellationToken ct) =>
        RespuestaIdentidad.Gestion(await verificar.EjecutarAsync(solicitud.Token, ct), HttpContext);

    /// <summary>Solicitar recuperación sin revelar si existe la cuenta.</summary>
    /// <remarks>Rol: público. Responde 202 incluso si no está verificada o supera las cuotas internas.</remarks>
    [HttpPost("recuperar", Name = "solicitarRecuperacion")]
    [ProducesResponseType(202), ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Recuperar(RecuperacionSolicitud solicitud, CancellationToken ct)
    {
        // RemoteIpAddress ya es la IP real: el host la toma de CF-Connecting-IP solo si viene del túnel.
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "sin-ip";
        return RespuestaIdentidad.Gestion(await recuperar.EjecutarAsync(solicitud.Correo, ip, ct), HttpContext);
    }

    /// <summary>Fijar contraseña con un enlace de recuperación o invitación.</summary>
    /// <remarks>Rol: público. Revoca las sesiones y reinicia el bloqueo de la cuenta.</remarks>
    [HttpPost("fijar-contrasena", Name = "fijarContrasenaConEnlace")]
    [ProducesResponseType(204), ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Fijar(FijarContrasenaSolicitud solicitud, CancellationToken ct) =>
        RespuestaIdentidad.Gestion(await fijar.EjecutarAsync(solicitud.Token, solicitud.NuevaContrasena, ct), HttpContext);
}
