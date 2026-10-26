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
        var entrada = new InicioSesionEntrada(solicitud.Correo, solicitud.Contrasena, solicitud.Canal, solicitud.TurnstileToken);
        var resultado = await iniciar.EjecutarAsync(entrada, ct);
        if (resultado.BloqueadoHastaUtc is { } finBloqueo) Response.Headers.RetryAfter = SegundosHasta(finBloqueo);
        if (resultado.Error is not null)
            return RespuestaIdentidad.Error(resultado.Error, resultado.Estado, HttpContext, resultado.BloqueadoHastaUtc);
        return Entregar(resultado.Sesion!, esPanel: solicitud.Canal == "panel");
    }

    /// <summary>Retry-After en segundos enteros, redondeado hacia arriba y nunca 0 (RFC 9110).</summary>
    private string SegundosHasta(DateTimeOffset instante)
    {
        var segundos = (int)Math.Ceiling((instante - reloj.GetUtcNow()).TotalSeconds);
        return Math.Max(1, segundos).ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>Renovar una sesión con su token de actualización.</summary>
    /// <remarks>Rol: público con refresh. La gracia de 60 segundos devuelve el mismo sucesor.</remarks>
    [AllowAnonymous, HttpPost("renovar", Name = "renovarSesion")]
    [ProducesResponseType(typeof(SesionEmitida), 200), ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> Renovar([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RenovarSolicitud? solicitud, CancellationToken ct)
    {
        // La app manda el token en el cuerpo; el panel no puede leer su cookie HttpOnly y manda el cuerpo vacío.
        var esPanel = string.IsNullOrEmpty(solicitud?.TokenRenovacion) && Request.Cookies.ContainsKey(CookiesSesion.Renovacion);
        var token = esPanel ? Request.Cookies[CookiesSesion.Renovacion] : solicitud?.TokenRenovacion;
        var resultado = await renovar.EjecutarAsync(token, ct);
        if (resultado.Error is not null) return RespuestaIdentidad.Error(resultado.Error, 401, HttpContext);
        return Entregar(resultado.Sesion!, esPanel);
    }

    /// <summary>Cerrar la familia de la sesión autenticada.</summary>
    /// <remarks>Rol: conductor o administrador. Borra ambas cookies en sus rutas originales.</remarks>
    [Authorize, HttpPost("cerrar-sesion", Name = "cerrarSesion")]
    [ProducesResponseType(204), ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> Cerrar([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RenovarSolicitud? solicitud, CancellationToken ct)
    {
        // El jti del JWT de acceso es el id de la familia de sesiones (FR-018).
        if (!Guid.TryParse(User.FindFirst("jti")?.Value, out var familia))
            return RespuestaIdentidad.Error("token_invalido", 401, HttpContext);
        var resultado = await cerrar.EjecutarAsync(familia, ct);
        CookiesSesion.Borrar(Response);
        return RespuestaIdentidad.Gestion(resultado, HttpContext);
    }

    /// <summary>La app recibe los tokens en el cuerpo; el panel, solo en cookies HttpOnly (FR-020).</summary>
    private OkObjectResult Entregar(SesionEmitida sesion, bool esPanel)
    {
        if (!esPanel) return Ok(sesion);
        CookiesSesion.Escribir(Response, sesion);
        return Ok(new SesionPanel(sesion.ExpiraAccesoUtc, sesion.SesionVenceUtc, sesion.Rol));
    }
}
