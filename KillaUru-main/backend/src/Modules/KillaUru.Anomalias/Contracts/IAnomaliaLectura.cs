using KillaUru.Anomalias.Contracts.Lecturas;
namespace KillaUru.Anomalias.Contracts;
/// <summary>Lecturas de Mapa y Dashboard sin acceso directo al esquema.</summary>
public interface IAnomaliaLectura
{
    /// <summary>Activas de las celdas indicadas, con su kilometraje fijo.</summary>
    Task<IReadOnlyList<AnomaliaVisible>> ActivasPorCeldasAsync(IReadOnlyCollection<long> celdas, CancellationToken ct);
    /// <summary>Combina rectángulo, tipo, severidades, fechas y estados.</summary>
    Task<IReadOnlyList<AnomaliaCambio>> FiltrarAsync(FiltroAnomalias filtro, CancellationToken ct);
    /// <summary>Cambios estrictamente posteriores al instante indicado, incluidos estados finales.</summary>
    Task<IReadOnlyList<AnomaliaCambio>> CambiosDesdeAsync(DateTimeOffset desde, CancellationToken ct);
    /// <summary>Celdas del índice del corredor; vacío antes de cargar la polilínea.</summary>
    IReadOnlySet<long> CeldasDelCorredor();
    /// <summary>Resumen del día UTC−5 sin identidades; renacimientos solo desde Expirada.</summary>
    Task<ResumenDia> ResumenDelDiaAsync(DateOnly diaLocal, CancellationToken ct);
}
