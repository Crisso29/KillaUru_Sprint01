using KillaUru.Dashboard.Domain;

namespace KillaUru.Dashboard.Infrastructure;

internal sealed class IndicadorDiario
{
    internal DateOnly  Dia       { get; init; }
    internal Indicador Indicador { get; init; }
    internal decimal   Valor     { get; init; }
}
