using KillaUru.Anomalias.Contracts.Lecturas;
namespace KillaUru.Anomalias.Application.Puertos;
/// <summary>Proyecciones independientes del almacenamiento para Mapa.</summary>
internal interface ILecturaAnomaliasRepository
{
    Task<IReadOnlyList<AnomaliaVisible>> ActivasAsync(IReadOnlyCollection<long> celdas, CancellationToken ct);
    Task<IReadOnlyList<AnomaliaCambio>> FiltrarAsync(FiltroAnomalias filtro, CancellationToken ct);
    Task<IReadOnlyList<AnomaliaCambio>> CambiosAsync(DateTimeOffset desde, CancellationToken ct);
}
