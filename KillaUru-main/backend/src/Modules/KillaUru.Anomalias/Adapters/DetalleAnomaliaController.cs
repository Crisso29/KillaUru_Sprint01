using KillaUru.Anomalias.Application.Administracion;
using Microsoft.AspNetCore.Mvc;
namespace KillaUru.Anomalias.Adapters;
/// <summary>Detalle e historial para administradores, sin identificadores de dispositivos.</summary>
[Route("api/v1/anomalies")]
internal sealed class DetalleAnomaliaController(ObtenerDetalleUseCase detalle, ListarTransicionesRecientesUseCase cambios) : AnomaliasControllerBase
{
    /// <summary>Consultar clasificación, reportes diarios y decisiones.</summary>
    /// <remarks>Rol: administrador. Los estados finales no tienen vencimiento visible.</remarks>
    [HttpGet("{id:guid}", Name = "obtenerDetalleAnomalia"), ProducesResponseType(typeof(DetalleAnomalia), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Obtener(Guid id, CancellationToken ct)
    {
        var resultado = await detalle.EjecutarAsync(id, ct);
        return resultado is null ? Error("no_encontrado", 404) : Ok(resultado);
    }
    /// <summary>Consultar las últimas transiciones, incluida la identidad del administrador.</summary>
    /// <remarks>Rol: administrador. Límite de 1 a 50, predeterminado 10.</remarks>
    [HttpGet("transiciones", Name = "listarTransicionesRecientes"), ProducesResponseType(typeof(TransicionReciente[]), 200)]
    public async Task<IActionResult> Transiciones([FromQuery] int limite = 10, CancellationToken ct = default) =>
        Ok(await cambios.EjecutarAsync(limite, ct));
}
