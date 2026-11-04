using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Contracts.Lecturas;
using KillaUru.Mapa.Domain;
using KillaUru.Mapa.Infrastructure;

namespace KillaUru.Mapa.Application;

/// <summary>
/// Consulta filtrada: llama IAnomaliaLectura.FiltrarAsync y aplica recorte demo para el anónimo.
/// FR-010, FR-011; sin caché ni ETag.
/// </summary>
internal sealed class ConsultarFiltradoUseCase(
    IAnomaliaLectura anomalias,
    ZonaDemo zona,
    TimeProvider reloj)
{
    internal async Task<ResultadoMapa> EjecutarAsync(
        Bbox? bbox,
        TipoAnomalia? tipo,
        int[]? severidades,
        DateTimeOffset? desdeUtc,
        DateTimeOffset? hastaUtc,
        EstadoAnomalia[] estados,
        bool recortarDemo,
        CancellationToken ct)
    {
        // Usamos un rectángulo muy grande si no hay bbox (corredor completo)
        double minLon = bbox?.MinLon ?? -180, minLat = bbox?.MinLat ?? -90;
        double maxLon = bbox?.MaxLon ?? 180,  maxLat = bbox?.MaxLat ?? 90;

        var filtro = new FiltroAnomalias(minLon, minLat, maxLon, maxLat,
            tipo, severidades, desdeUtc, hastaUtc, estados);

        var anomaliasEncontradas = await anomalias.FiltrarAsync(filtro, ct);

        IReadOnlyList<AnomaliaCambio> visibles = recortarDemo
            ? anomaliasEncontradas.Where(a => zona.Contiene(a.Latitud, a.Longitud)).ToList()
            : anomaliasEncontradas;

        bool conEstado = estados.Length == 5 || estados.Any(e =>
            e is EstadoAnomalia.PendienteConfirmacion or EstadoAnomalia.Expirada
                or EstadoAnomalia.Reparada or EstadoAnomalia.Descartada);

        var generadoUtc = reloj.GetUtcNow();
        var cuerpo = SerializadorGeoJson.SerializarColeccionFiltrada(visibles, [], generadoUtc, conEstado);
        return new ResultadoMapa.Contenido(cuerpo, ETag: null, Publico: false);
    }
}
