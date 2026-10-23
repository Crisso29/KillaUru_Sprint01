using KillaUru.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
}
