using System.Collections.Frozen;
using KillaUru.Shared.Domain;
namespace KillaUru.Anomalias.Domain;

/// <summary>Polilínea inmutable indexada; no consulta base de datos durante la recepción.</summary>
internal sealed class CorredorVia
{
    private readonly SegmentoVia[] segmentos;
    private readonly FrozenDictionary<long, SegmentoVia[]> indice;
    internal IReadOnlySet<long> Celdas { get; }
    internal double LongitudKm { get; }
    internal CorredorVia(IReadOnlyList<Coordenada> vertices, double margenMetros)
    {
        segmentos = CrearSegmentos(vertices);
        indice = segmentos.SelectMany(s => IndiceCorredor.Celdas(s, margenMetros).Select(c => (Celda: c, Segmento: s)))
            .GroupBy(x => x.Celda).ToFrozenDictionary(g => g.Key, g => g.Select(x => x.Segmento).ToArray());
        Celdas = indice.Keys.ToFrozenSet();
        LongitudKm = segmentos.Sum(s => s.Longitud) / 1000;
    }

    internal double DistanciaMetros(Coordenada p) => Proyectar(p).Distancia;
    internal double KmSobreVia(Coordenada p) => Proyectar(p).Km;
    private (double Distancia, double Km) Proyectar(Coordenada p)
    {
        if (!indice.TryGetValue(Celda.De(p).Id, out var cercanos)) return (double.PositiveInfinity, double.NaN);
        return cercanos.Select(s => s.Proyectar(p)).MinBy(x => x.Distancia);
    }

    private static SegmentoVia[] CrearSegmentos(IReadOnlyList<Coordenada> vertices)
    {
        var resultado = new List<SegmentoVia>();
        var metros = 0.0;
        for (var i = 1; i < vertices.Count; i++)
        {
            var segmento = new SegmentoVia(vertices[i - 1], vertices[i], metros);
            resultado.Add(segmento);
            metros += segmento.Longitud;
        }
        return resultado.ToArray();
    }
}
