using System.Globalization;
using KillaUru.Identidad.Adapters.Dtos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Application.Sesion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace KillaUru.Identidad.Adapters;

/// <summary>App usa cuerpo; panel usa cookies. La renovación no requiere un acceso aún vigente.</summary>
[Route("api/v1/auth")]
internal sealed class SesionController(IniciarSesionUseCase iniciar, RenovarSesionUseCase renovar,
    CerrarSesionUseCase cerrar, TimeProvider reloj) : IdentidadControllerBase
{
    /// <summary>Iniciar sesión en la app o el panel.</summary>
    /// <remarks>Rol: público. Panel exige anti-bot y administrador; solo el panel ve el bloqueo.</remarks>
    [AllowAnonymous, HttpPost("inicio-sesion", Name = "iniciarSesion")]
    [ProducesResponseType(typeof(SesionEmitida), 200), ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 403), ProducesResponseType(typeof(ProblemDetails), 423)]
    public async Task<IActionResult> Iniciar(InicioSesionSolicitud solicitud, CancellationToken ct)
    {
        var resultado = await iniciar.EjecutarAsync(new(solicitud.Correo, solicitud.Contrasena, solicitud.Canal, solicitud.TurnstileToken), ct);
        if (resultado.BloqueadoHastaUtc is { } fin)
            Response.Headers.RetryAfter = Math.Max(1, (int)Math.Ceiling((fin - reloj.GetUtcNow()).TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        return resultado.Error is null ? Entregar(resultado.Sesion!, solicitud.Canal == "panel")
            : RespuestaIdentidad.Error(resultado.Error, resultado.Estado, HttpContext, resultado.BloqueadoHastaUtc);
    }

    /// <summary>Renovar una sesión con su token de actualización.</summary>
    /// <remarks>Rol: público con refresh. La gracia de 60 segundos devuelve el mismo sucesor.</remarks>
    [AllowAnonymous, HttpPost("renovar", Name = "renovarSesion")]
    [ProducesResponseType(typeof(SesionEmitida), 200), ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> Renovar([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RenovarSolicitud? solicitud, CancellationToken ct)
    {
        var panel = string.IsNullOrEmpty(solicitud?.TokenRenovacion) && Request.Cookies.ContainsKey("ku_renovacion");
        var token = panel ? Request.Cookies["ku_renovacion"] : solicitud?.TokenRenovacion;
        var resultado = await renovar.EjecutarAsync(token, ct);
        return resultado.Error is null ? Entregar(resultado.Sesion!, panel) : RespuestaIdentidad.Error(resultado.Error, 401, HttpContext);
    }

    /// <summary>Cerrar la familia de la sesión autenticada.</summary>
    /// <remarks>Rol: conductor o administrador. Borra ambas cookies en sus rutas originales.</remarks>
    [Authorize, HttpPost("cerrar-sesion", Name = "cerrarSesion")]
    [ProducesResponseType(204), ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> Cerrar([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RenovarSolicitud? solicitud, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirst("jti")?.Value, out var familia)) return RespuestaIdentidad.Error("token_invalido", 401, HttpContext);
        var resultado = await cerrar.EjecutarAsync(familia, ct);
        CookiesSesion.Borrar(Response);
        return RespuestaIdentidad.Gestion(resultado, HttpContext);
    }

    private OkObjectResult Entregar(SesionEmitida sesion, bool panel)
    {
        if (!panel) return Ok(sesion);
        CookiesSesion.Escribir(Response, sesion);
        return Ok(new SesionPanel(sesion.ExpiraAccesoUtc, sesion.SesionVenceUtc, sesion.Rol));
    }
}
