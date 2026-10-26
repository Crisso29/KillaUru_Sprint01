using KillaUru.Shared.Domain;
namespace KillaUru.Anomalias.Domain;

/// <summary>Índice conservador de cajas por celda; la distancia exacta decide pertenencia a la franja.</summary>
internal static class IndiceCorredor
{
    internal static IEnumerable<long> Celdas(SegmentoVia segmento, double margen)
    {
        // Holgura angular: incluye los bordes; el corredor está acotado a las latitudes de Perú.
        var dLat = margen / 100000;
        var dLon = dLat / Math.Cos(Math.Max(Math.Abs(segmento.Inicio.Latitud), Math.Abs(segmento.Fin.Latitud)) * Math.PI / 180);
        var minimo = Coordenada.Crear(Math.Min(segmento.Inicio.Latitud, segmento.Fin.Latitud) - dLat,
            Math.Min(segmento.Inicio.Longitud, segmento.Fin.Longitud) - dLon)!.Value;
        var maximo = Coordenada.Crear(Math.Max(segmento.Inicio.Latitud, segmento.Fin.Latitud) + dLat,
            Math.Max(segmento.Inicio.Longitud, segmento.Fin.Longitud) + dLon)!.Value;
        var (lat, lon, maxLat, maxLon) = Celda.De(minimo).Limites();
        var pasoLat = maxLat - lat; var pasoLon = maxLon - lon;
        for (var y = lat + pasoLat / 2; y <= maximo.Latitud + pasoLat / 2; y += pasoLat)
        {
            for (var x = lon + pasoLon / 2; x <= maximo.Longitud + pasoLon / 2; x += pasoLon)
                yield return Celda.De(Coordenada.Crear(y, x)!.Value).Id;
        }
    }
}
