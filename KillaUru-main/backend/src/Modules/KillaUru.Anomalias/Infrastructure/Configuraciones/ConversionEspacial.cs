using KillaUru.Shared.Domain;
using NetTopologySuite.Geometries;
namespace KillaUru.Anomalias.Infrastructure.Configuraciones;
/// <summary>WGS84 mantiene orden longitud/latitud; el dominio conserva su value object.</summary>
internal static class ConversionEspacial
{
    internal static Point Punto(Coordenada p) => new(p.Longitud, p.Latitud) { SRID = 4326 };
    internal static Coordenada Coordenada(Point p) => Shared.Domain.Coordenada.Crear(p.Y, p.X)!.Value;
}
