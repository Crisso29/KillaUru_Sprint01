using KillaUru.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace KillaUru.Integracion.Tests.Infraestructura;

/// <summary>
/// Endpoints que existen solo en las pruebas: ejercitan la seguridad común del host (políticas de tasa,
/// roles, errores) igual que lo harán los controladores de los módulos.
/// </summary>
[ApiController]
[Route("api/v1/prueba")]
public sealed class ControladorDePrueba : ControllerBase
{
    [HttpGet("publica")]
    [EnableRateLimiting("consultas")]
    public IActionResult Publica() => Ok(User.FindFirst("rol")?.Value ?? Roles.Anonimo);

    [HttpPost("lote")]
    [EnableRateLimiting("lotes")]
    public IActionResult Lote() => Ok();

    [HttpPost("registro")]
    [EnableRateLimiting("registro")]
    public IActionResult Registro() => Ok();

    [HttpGet("conductor")]
    [Authorize(Roles = Roles.ConductorRegistrado)]
    public IActionResult SoloConductor() => Ok();

    [HttpGet("admin")]
    [Authorize(Roles = Roles.Administrador)]
    public IActionResult SoloAdministrador() => Ok();

    [HttpGet("error")]
    public IActionResult Falla() => throw new InvalidOperationException($"detalle interno que nunca debe salir ({Request.Path})");

    // Simula un endpoint que intenta hacer pública una respuesta autenticada.
    [HttpGet("cache-publica")]
    public IActionResult CachePublica()
    {
        Response.Headers.CacheControl = "public, max-age=60";
        return Ok();
    }

    private static readonly Action<ILogger, Exception?> _logVerificacion =
        LoggerMessage.Define(LogLevel.Information, default, "Verificacion de filtro de propiedades en pipeline");

    // Pushea propiedades al LogContext y emite un log — permite verificar que FiltroDatosPersonales
    // las elimina en el pipeline real (T067).
    [HttpPost("log-contexto")]
    public IActionResult LogContexto(
        [FromBody] LogContextoBody cuerpo,
        [FromServices] ILogger<ControladorDePrueba> logger)
    {
        using var d1 = cuerpo.Propiedades.TryGetValue("Seudonimo", out var s) ? LogContext.PushProperty("Seudonimo", s) : null;
        using var d2 = cuerpo.Propiedades.TryGetValue("Sub", out var sub) ? LogContext.PushProperty("Sub", sub) : null;
        using var d3 = cuerpo.Propiedades.TryGetValue("LoteId", out var l) ? LogContext.PushProperty("LoteId", l) : null;
        _logVerificacion(logger, null);
        return Ok();
    }
}

public sealed class LogContextoBody
{
    public Dictionary<string, string> Propiedades { get; init; } = [];
}
