using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Application.Procesamiento;
using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Contracts.Lecturas;
namespace KillaUru.Anomalias.Application.Lecturas;
/// <summary>Fachada de lecturas para otros módulos; el recorte por rol pertenece a Mapa.</summary>
internal sealed class AnomaliaLectura(ILecturaAnomaliasRepository repo, IResumenDiaRepository resumen, EstadoCorredor corredor) : IAnomaliaLectura
{
    public Task<IReadOnlyList<AnomaliaVisible>> ActivasPorCeldasAsync(IReadOnlyCollection<long> celdas, CancellationToken ct) => repo.ActivasAsync(celdas, ct);
    public Task<IReadOnlyList<AnomaliaCambio>> FiltrarAsync(FiltroAnomalias filtro, CancellationToken ct) => repo.FiltrarAsync(filtro, ct);
    public Task<IReadOnlyList<AnomaliaCambio>> CambiosDesdeAsync(DateTimeOffset desde, CancellationToken ct) => repo.CambiosAsync(desde, ct);
    public IReadOnlySet<long> CeldasDelCorredor() => corredor.Actual?.Celdas ?? new HashSet<long>();
    public Task<ResumenDia> ResumenDelDiaAsync(DateOnly diaLocal, CancellationToken ct) =>
        resumen.ObtenerAsync(diaLocal, (int)Math.Ceiling(corredor.Actual?.LongitudKm ?? 0), ct);
}
