using KillaUru.Anomalias.Application.Administracion;
namespace KillaUru.Anomalias.Application.Puertos;
/// <summary>Listas de decisión independientes de las lecturas del mapa.</summary>
internal interface ILecturaAdministracionRepository
{
    Task<IReadOnlyList<ElementoPorRevisar>> PorRevisarAsync(CancellationToken ct);
    Task<IReadOnlyList<ElementoPendiente>> PendientesAsync(CancellationToken ct);
    Task<IReadOnlyList<TransicionReciente>> TransicionesAsync(int limite, CancellationToken ct);
}
