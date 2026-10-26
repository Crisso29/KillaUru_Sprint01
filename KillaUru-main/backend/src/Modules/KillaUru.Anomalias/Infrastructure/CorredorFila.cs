using NetTopologySuite.Geometries;
namespace KillaUru.Anomalias.Infrastructure;
/// <summary>Representación espacial de infraestructura; NTS no cruza hacia Domain.</summary>
internal sealed class CorredorFila
{
    public short Id { get; set; }
    public LineString Traza { get; set; } = null!;
}
