using KillaUru.Mapa.Application.Puertos;
using KillaUru.Mapa.Domain;
using KillaUru.Mapa.Infrastructure;

namespace KillaUru.Mapa.Application;

/// <summary>
/// Resuelve la consulta de mapa por celdas: ETag, 304 y ensamblado de bytes pre-serializados.
/// FR-006, FR-007, FR-008, FR-009; diseño-interno §4.
/// </summary>
internal sealed class ConsultarPorCeldasUseCase(
    CorredorMapa corredor,
    IFuenteCeldas fuente,
    CalculadorETag etag,
    IMetricasMapa metricas,
    TimeProvider reloj)
{
    internal async Task<ResultadoMapa> EjecutarAsync(
        Alcance alcance, Bbox? bbox, string? ifNoneMatch, CancellationToken ct)
    {
        var celdas = alcance == Alcance.Anonimo
            ? corredor.CeldasDeAnonimo(bbox)
            : corredor.CeldasDeRegistrado(bbox);

        var celdasOrdenadas = celdas.Order().ToList();
        var etagActual = etag.Calcular(alcance, celdasOrdenadas);

        if (etagActual == ifNoneMatch)
        {
            metricas.RegistrarCache("no_modificado");
            return new ResultadoMapa.NoModificado(etagActual);
        }

        var fragmentos = new List<byte[]>(celdasOrdenadas.Count);
        bool hayFallo = false;
        foreach (var celda in celdasOrdenadas)
        {
            var bytes = await fuente.ObtenerAsync(celda, alcance, ct);
            if (bytes.Length > 0) fragmentos.Add(bytes);
            // fallo = no podemos distinguir aquí sin cambiar IFuenteCeldas;
            // la infraestructura registra hits/misses directamente
        }
        _ = hayFallo; // suprimido; la cache decorator registra sus propias métricas

        var generadoUtc = reloj.GetUtcNow();
        var cuerpo = SerializadorGeoJson.EnsamblarColeccion(fragmentos, generadoUtc);
        metricas.RegistrarCache("acierto");

        return new ResultadoMapa.Contenido(cuerpo, etagActual, Publico: alcance == Alcance.Anonimo);
    }
}
