using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Contracts.Lecturas;
using KillaUru.Mapa.Application;
using KillaUru.Mapa.Application.Puertos;
using KillaUru.Mapa.Domain;

namespace KillaUru.Mapa.Infrastructure;

/// <summary>
/// Implementación base de <see cref="IFuenteCeldas"/>: consulta Anomalías y serializa a bytes.
/// Las celdas de borde se filtran punto a punto para el alcance anónimo (FR-004).
/// </summary>
internal sealed class FuenteCeldasAnomalias(IAnomaliaLectura anomalias, ZonaDemo zona) : IFuenteCeldas
{
    public async Task<byte[]> ObtenerAsync(long celda, Alcance alcance, CancellationToken ct)
    {
        var activas = await anomalias.ActivasPorCeldasAsync([celda], ct);
        if (activas.Count == 0) return [];

        IReadOnlyList<AnomaliaVisible> visibles = alcance == Alcance.Anonimo
            ? activas.Where(a => zona.Contiene(a.Latitud, a.Longitud)).ToList()
            : activas;

        return SerializadorGeoJson.SerializarCelda(visibles);
    }
}
