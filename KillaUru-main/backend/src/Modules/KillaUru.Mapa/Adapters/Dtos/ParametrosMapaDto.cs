using KillaUru.Anomalias.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace KillaUru.Mapa.Adapters.Dtos;

/// <summary>Parámetros de la petición GET /api/v1/anomalies (data-model §3).</summary>
internal sealed class ParametrosMapaDto
{
    [FromQuery(Name = "bbox")]
    public string? Bbox { get; init; }

    [FromQuery(Name = "tipo")]
    public TipoAnomalia? Tipo { get; init; }

    [FromQuery(Name = "severidad")]
    public int[]? Severidades { get; init; }

    [FromQuery(Name = "desde")]
    public DateTimeOffset? Desde { get; init; }

    [FromQuery(Name = "hasta")]
    public DateTimeOffset? Hasta { get; init; }

    [FromQuery(Name = "estado")]
    public EstadoAnomalia[]? Estados { get; init; }

    [FromQuery(Name = "updatedSince")]
    public DateTimeOffset? UpdatedSince { get; init; }
}
