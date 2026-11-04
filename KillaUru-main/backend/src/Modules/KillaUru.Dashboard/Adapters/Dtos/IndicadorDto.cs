using KillaUru.Dashboard.Application;

namespace KillaUru.Dashboard.Adapters.Dtos;

/// <summary>Medida de un indicador del dashboard con valor, comparación y serie temporal.</summary>
internal sealed record IndicadorDto(
    decimal? Valor,
    decimal? Comparacion,
    decimal? Variacion,
    string   Unidad,
    IReadOnlyList<decimal?> Serie)
{
    internal static IndicadorDto Desde(IndicadorResumen r) =>
        new(r.Valor, r.Comparacion, r.Variacion, r.Unidad, r.Serie);
}
