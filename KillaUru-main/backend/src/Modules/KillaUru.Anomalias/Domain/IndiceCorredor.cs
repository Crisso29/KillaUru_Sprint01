using KillaUru.Shared.Domain;

namespace KillaUru.Anomalias.Domain;

/// <summary>Índice conservador de cajas por celda; la distancia exacta decide pertenencia a la franja.</summary>
internal static class IndiceCorredor
{
    /// <summary>
    /// Un grado de latitud mide unos 111 km; usar 100 km agranda la holgura angular, así que la caja
    /// nunca queda corta (sobran celdas, nunca faltan).
    /// </summary>
    private const double MetrosPorGradoConservador = 100_000;

    /// <summary>Celdas que toca la caja del segmento ampliada en <paramref name="margen"/> metros.</summary>
    internal static IEnumerable<long> Celdas(SegmentoVia segmento, double margen)
    {
        var (minimo, maximo) = CajaAmpliada(segmento, margen);
        var (lat, lon, maxLat, maxLon) = Celda.De(minimo).Limites();
        var pasoLat = maxLat - lat;
        var pasoLon = maxLon - lon;
        // Se recorre el centro de cada celda: así el redondeo de los bordes no salta ninguna.
        for (var y = lat + pasoLat / 2; y <= maximo.Latitud + pasoLat / 2; y += pasoLat)
        {
            for (var x = lon + pasoLon / 2; x <= maximo.Longitud + pasoLon / 2; x += pasoLon)
                yield return Celda.De(Coordenada.Crear(y, x)!.Value).Id;
        }
    }

    /// <summary>Caja del segmento con holgura; la longitud se corrige por el coseno de la latitud (el corredor está en Perú).</summary>
    private static (Coordenada Minimo, Coordenada Maximo) CajaAmpliada(SegmentoVia segmento, double margen)
    {
        var (inicio, fin) = (segmento.Inicio, segmento.Fin);
        var holguraLat = margen / MetrosPorGradoConservador;
        var latitudExtrema = Math.Max(Math.Abs(inicio.Latitud), Math.Abs(fin.Latitud));
        var holguraLon = holguraLat / Math.Cos(latitudExtrema * Math.PI / 180);
        var minimo = Coordenada.Crear(
            Math.Min(inicio.Latitud, fin.Latitud) - holguraLat, Math.Min(inicio.Longitud, fin.Longitud) - holguraLon)!.Value;
        var maximo = Coordenada.Crear(
            Math.Max(inicio.Latitud, fin.Latitud) + holguraLat, Math.Max(inicio.Longitud, fin.Longitud) + holguraLon)!.Value;
        return (minimo, maximo);
    }
}
