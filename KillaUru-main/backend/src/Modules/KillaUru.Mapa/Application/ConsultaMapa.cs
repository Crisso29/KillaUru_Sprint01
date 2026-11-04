using KillaUru.Anomalias.Contracts;
using KillaUru.Mapa.Domain;

namespace KillaUru.Mapa.Application;

/// <summary>Parámetros de entrada de una consulta al mapa (data-model §3).</summary>
internal sealed record ConsultaMapa(
    string Rol,
    Bbox? Bbox,
    TipoAnomalia? Tipo,
    int[]? Severidades,
    DateTimeOffset? DesdeUtc,
    DateTimeOffset? HastaUtc,
    EstadoAnomalia[]? Estados,
    DateTimeOffset? UpdatedSince,
    string? IfNoneMatch)
{
    internal bool HayFiltros =>
        Tipo.HasValue || (Severidades is { Length: > 0 }) ||
        DesdeUtc.HasValue || HastaUtc.HasValue;

    internal bool HayEstado => Estados is { Length: > 0 };
    internal bool HayUpdatedSince => UpdatedSince.HasValue;
}
