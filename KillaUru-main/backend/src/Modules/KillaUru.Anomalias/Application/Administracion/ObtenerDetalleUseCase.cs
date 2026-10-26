using KillaUru.Anomalias.Application.Puertos;
namespace KillaUru.Anomalias.Application.Administracion;
/// <summary>Consulta administrativa ObtenerDetalle; no modifica anomalías.</summary>
internal sealed class ObtenerDetalleUseCase(IDetalleAnomaliaRepository repo)
{
    internal Task<DetalleAnomalia?> EjecutarAsync(Guid id, CancellationToken ct) => repo.ObtenerAsync(id, ct);
}
