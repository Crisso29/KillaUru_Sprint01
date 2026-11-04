using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Contracts.Lecturas;
using KillaUru.Mapa.Domain;
using KillaUru.Mapa.Infrastructure;

namespace KillaUru.Mapa.Application;

/// <summary>
/// Cambios desde un instante: altas visibles por rol y bajas como «eliminada:true».
/// FR-012, FR-013; R-012; T-SEG16.
/// </summary>
internal sealed class ConsultarCambiosUseCase(
    IAnomaliaLectura anomalias,
    ZonaDemo zona,
    TimeProvider reloj)
{
    internal async Task<ResultadoMapa> EjecutarAsync(
        DateTimeOffset updatedSince,
        bool recortarDemo,
        bool esAdmin,
        CancellationToken ct)
    {
        var cambios = await anomalias.CambiosDesdeAsync(updatedSince, ct);
        var generadoUtc = reloj.GetUtcNow();

        var altas = new List<AnomaliaCambio>();
        var bajas = new List<Guid>();

        foreach (var c in cambios)
        {
            bool estaActivo = esAdmin || c.Estado == EstadoAnomalia.Activa;
            bool dentroDemo = !recortarDemo || zona.Contiene(c.Latitud, c.Longitud);

            if (estaActivo && dentroDemo)
                altas.Add(c);
            else if (!estaActivo && dentroDemo)
                bajas.Add(c.Id);
        }

        var cuerpo = SerializadorGeoJson.SerializarColeccionFiltrada(altas, bajas, generadoUtc, esAdmin);
        return new ResultadoMapa.Contenido(cuerpo, ETag: null, Publico: !esAdmin);
    }
}
