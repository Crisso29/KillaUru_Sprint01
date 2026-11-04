namespace KillaUru.Mapa.Domain;

/// <summary>Polígono geográfico con algoritmos de punto-en-polígono e intersección con rectángulo.</summary>
internal sealed class PoligonoGeo
{
    private readonly double[] _lats;
    private readonly double[] _lons;

    /// <param name="coordenadas">Vértices en formato GeoJSON: cada elemento es [lon, lat].</param>
    internal PoligonoGeo(double[][] coordenadas)
    {
        _lats = new double[coordenadas.Length];
        _lons = new double[coordenadas.Length];
        for (int i = 0; i < coordenadas.Length; i++)
        {
            _lons[i] = coordenadas[i][0];
            _lats[i] = coordenadas[i][1];
        }
    }

    /// <summary>Rayo horizontal hacia la derecha (even-odd rule).</summary>
    internal bool Contiene(double lat, double lon)
    {
        bool dentro = false;
        int n = _lats.Length;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if ((_lats[i] > lat) != (_lats[j] > lat) &&
                lon < (_lons[j] - _lons[i]) * (lat - _lats[i]) / (_lats[j] - _lats[i]) + _lons[i])
            {
                dentro = !dentro;
            }
        }
        return dentro;
    }

    /// <summary>Devuelve <c>true</c> si el polígono se superpone con el rectángulo (minLat, minLon, maxLat, maxLon).</summary>
    internal bool Intersecta(double minLat, double minLon, double maxLat, double maxLon)
    {
        if (Contiene(minLat, minLon) || Contiene(minLat, maxLon)
         || Contiene(maxLat, minLon) || Contiene(maxLat, maxLon))
        {
            return true;
        }

        int n = _lats.Length;
        for (int i = 0; i < n; i++)
        {
            if (_lats[i] >= minLat && _lats[i] <= maxLat &&
                _lons[i] >= minLon && _lons[i] <= maxLon)
            {
                return true;
            }
        }

        (double x1, double y1, double x2, double y2)[] ladosBbox =
        [
            (minLon, minLat, maxLon, minLat),
            (maxLon, minLat, maxLon, maxLat),
            (maxLon, maxLat, minLon, maxLat),
            (minLon, maxLat, minLon, minLat),
        ];

        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            foreach (var lado in ladosBbox)
            {
                if (SegmentosSeCortan(_lons[i], _lats[i], _lons[j], _lats[j],
                                      lado.x1, lado.y1, lado.x2, lado.y2))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static double Cruce(double ux, double uy, double vx, double vy) => ux * vy - uy * vx;

    private static bool SegmentosSeCortan(double ax, double ay, double bx, double by,
                                           double cx, double cy, double dx, double dy)
    {
        double d1 = Cruce(dx - cx, dy - cy, ax - cx, ay - cy);
        double d2 = Cruce(dx - cx, dy - cy, bx - cx, by - cy);
        double d3 = Cruce(bx - ax, by - ay, cx - ax, cy - ay);
        double d4 = Cruce(bx - ax, by - ay, dx - ax, dy - ay);
        return (d1 > 0 ? 1 : d1 < 0 ? -1 : 0) != (d2 > 0 ? 1 : d2 < 0 ? -1 : 0) &&
               (d3 > 0 ? 1 : d3 < 0 ? -1 : 0) != (d4 > 0 ? 1 : d4 < 0 ? -1 : 0);
    }
}
