namespace KillaUru.Dashboard.Domain;

internal sealed record MedidaIndicador(
    decimal? Valor,
    decimal? Comparacion,
    decimal? Variacion,
    IReadOnlyList<decimal?> Serie);
