using KillaUru.Anomalias.Application.Puertos;
namespace KillaUru.Anomalias.Application.Administracion;
/// <summary>Consulta administrativa ListarPorRevisar; no modifica anomalías.</summary>
internal sealed class ListarPorRevisarUseCase(ILecturaAdministracionRepository repo)
{
    internal Task<IReadOnlyList<ElementoPorRevisar>> EjecutarAsync(CancellationToken ct) => repo.PorRevisarAsync(ct);
}
