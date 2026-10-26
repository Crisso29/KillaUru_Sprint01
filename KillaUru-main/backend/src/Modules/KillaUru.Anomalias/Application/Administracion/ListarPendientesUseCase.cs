using KillaUru.Anomalias.Application.Puertos;
namespace KillaUru.Anomalias.Application.Administracion;
/// <summary>Consulta administrativa ListarPendientes; no modifica anomalías.</summary>
internal sealed class ListarPendientesUseCase(ILecturaAdministracionRepository repo)
{
    internal Task<IReadOnlyList<ElementoPendiente>> EjecutarAsync(CancellationToken ct) => repo.PendientesAsync(ct);
}
