using KillaUru.Anomalias.Application.Administracion;
namespace KillaUru.Anomalias.Application.Puertos;
/// <summary>Detalle de administración sin entidades persistentes en la respuesta.</summary>
internal interface IDetalleAnomaliaRepository
{
    Task<DetalleAnomalia?> ObtenerAsync(Guid id, CancellationToken ct);
}
