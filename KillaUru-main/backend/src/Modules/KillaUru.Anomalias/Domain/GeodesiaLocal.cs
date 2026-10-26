using KillaUru.Shared.Domain;
namespace KillaUru.Anomalias.Domain;

/// <summary>Haversine y proyección local; constantes físicas, independientes de umbrales de negocio.</summary>
internal static class GeodesiaLocal
{
    private const double RadioTerrestre = 6371008.8;
    private const double Radianes = Math.PI / 180;
    internal static double Distancia(Coordenada a, Coordenada b)
    {
        var lat = Math.Sin((b.Latitud - a.Latitud) * Radianes / 2);
        var lon = Math.Sin((b.Longitud - a.Longitud) * Radianes / 2);
        var h = lat * lat + Math.Cos(a.Latitud * Radianes) * Math.Cos(b.Latitud * Radianes) * lon * lon;
        return 2 * RadioTerrestre * Math.Asin(Math.Sqrt(Math.Clamp(h, 0, 1)));
    }

    internal static (double Distancia, double Fraccion) Proyectar(Coordenada p, Coordenada a, Coordenada b)
    {
        var escala = RadioTerrestre * Radianes;
        var coseno = Math.Cos((a.Latitud + b.Latitud) * Radianes / 2);
        var x = (b.Longitud - a.Longitud) * escala * coseno;
        var y = (b.Latitud - a.Latitud) * escala;
        var px = (p.Longitud - a.Longitud) * escala * coseno;
        var py = (p.Latitud - a.Latitud) * escala;
        var largo2 = x * x + y * y;
        var t = largo2 == 0 ? 0 : Math.Clamp((px * x + py * y) / largo2, 0, 1);
        return (Math.Sqrt(Math.Pow(px - t * x, 2) + Math.Pow(py - t * y, 2)), t);
    }
}
