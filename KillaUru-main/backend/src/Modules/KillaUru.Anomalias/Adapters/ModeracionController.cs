using KillaUru.Anomalias.Application.Administracion;
using Microsoft.AspNetCore.Mvc;
namespace KillaUru.Anomalias.Adapters;
/// <summary>Decisiones de RN-07; las listas son de solo lectura.</summary>
internal sealed class ModeracionController(ModerarAnomaliaUseCase moderar, ListarPorRevisarUseCase revisar, ListarPendientesUseCase pendientes) : AnomaliasControllerBase
{
    /// <summary>Aplicar una decisión administrativa y devolver el detalle.</summary>
    /// <remarks>Rol: administrador. La decisión se audita en la misma transacción.</remarks>
    [HttpPost("api/v1/anomalies/{id:guid}/moderacion", Name = "moderarAnomalia"), ProducesResponseType(typeof(DetalleAnomalia), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400), ProducesResponseType(typeof(ProblemDetails), 404), ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Moderar(Guid id, ComandoModeracion solicitud, CancellationToken ct)
    {
        var r = await moderar.EjecutarAsync(id, solicitud, Actor, ct);
        return r.Error is null ? Ok(r.Detalle) : Error(r.Error, r.Error == "no_encontrado" ? 404 : r.Error == "datos_invalidos" ? 400 : 409);
    }
    /// <summary>Listar descartadas que requieren una nueva decisión.</summary>
    /// <remarks>Rol: administrador. Incluye severidad y cantidad de vehículos de revisión.</remarks>
    [HttpGet("api/v1/moderacion/por-revisar", Name = "listarPorRevisar"), ProducesResponseType(typeof(ElementoPorRevisar[]), 200)]
    public async Task<IActionResult> PorRevisar(CancellationToken ct) => Ok(await revisar.EjecutarAsync(ct));
    /// <summary>Listar anomalías que esperan confirmación, ordenadas por vencimiento.</summary>
    /// <remarks>Rol: administrador. No confirma anomalías manualmente.</remarks>
    [HttpGet("api/v1/moderacion/pendientes", Name = "listarPendientes"), ProducesResponseType(typeof(ElementoPendiente[]), 200)]
    public async Task<IActionResult> Pendientes(CancellationToken ct) => Ok(await pendientes.EjecutarAsync(ct));
}
