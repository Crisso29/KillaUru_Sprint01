using KillaUru.Shared.Domain;
namespace KillaUru.Anomalias.Domain;

/// <summary>Segmento inmutable con distancia acumulada para evitar recorrer toda la polilínea.</summary>
internal sealed record SegmentoVia(Coordenada Inicio, Coordenada Fin, double MetrosPrevios)
{
    internal double Longitud => GeodesiaLocal.Distancia(Inicio, Fin);
    internal (double Distancia, double Km) Proyectar(Coordenada punto)
    {
        var (distancia, fraccion) = GeodesiaLocal.Proyectar(punto, Inicio, Fin);
        return (distancia, (MetrosPrevios + Longitud * fraccion) / 1000);
    }
}
