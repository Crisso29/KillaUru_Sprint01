using KillaUru.Mapa.Domain;

namespace KillaUru.Mapa.Application.Puertos;

/// <summary>Puerto para obtener los bytes GeoJSON pre-serializados de una celda (ADR-006).</summary>
internal interface IFuenteCeldas
{
    /// <summary>Devuelve los bytes JSON de las <i>features</i> de la celda, vacío si no hay anomalías.</summary>
    Task<byte[]> ObtenerAsync(long celda, Alcance alcance, CancellationToken ct);
}
