using KillaUru.Dashboard.Domain;

namespace KillaUru.Dashboard.Application;

internal sealed record IndicadorResumen(
    decimal? Valor,
    decimal? Comparacion,
    decimal? Variacion,
    IReadOnlyList<decimal?> Serie,
    string Unidad)
{
    internal static IndicadorResumen Desde(MedidaIndicador m, string unidad) =>
        new(m.Valor, m.Comparacion, m.Variacion, m.Serie, unidad);
}
