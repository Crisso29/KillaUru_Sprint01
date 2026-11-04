using KillaUru.Anomalias.Contracts;
using KillaUru.Mapa.Domain;
using KillaUru.Shared.Domain;

namespace KillaUru.Mapa.Application;

/// <summary>
/// Celdas del corredor y su clasificación demo; se inicializa una vez (perezoso).
/// Envuelve <see cref="IAnomaliaLectura.CeldasDelCorredor"/> (spec 006 §CorredorMapa).
/// </summary>
internal sealed class CorredorMapa(IAnomaliaLectura anomalias, ZonaDemo zona)
{
    private IReadOnlyDictionary<long, ClaseCelda>? _clasificacion;
    private readonly object _candado = new();

    internal IReadOnlySet<long> Celdas => anomalias.CeldasDelCorredor();

    private IReadOnlyDictionary<long, ClaseCelda> Clasificacion
    {
        get
        {
            if (_clasificacion is not null) return _clasificacion;
            lock (_candado)
            {
                if (_clasificacion is not null) return _clasificacion;
                var dict = new Dictionary<long, ClaseCelda>();
                foreach (var id in anomalias.CeldasDelCorredor())
                    dict[id] = zona.Clasificar(new Celda(id).Limites());
                _clasificacion = dict;
            }
            return _clasificacion;
        }
    }

    /// <summary>Celdas del corredor que intersecan con el bbox dado (o todas si bbox es null).</summary>
    internal IReadOnlyList<long> CeldasDeRegistrado(Bbox? bbox)
    {
        var todas = Celdas;
        if (bbox is null) return todas.ToList();
        return todas.Where(id => bbox.Intersecta(new Celda(id).Limites())).ToList();
    }

    /// <summary>Celdas del corredor que están dentro o en borde del demo, filtradas por bbox.</summary>
    internal IReadOnlyList<long> CeldasDeAnonimo(Bbox? bbox)
    {
        var clasif = Clasificacion;
        return Celdas
            .Where(id => clasif.TryGetValue(id, out var c) && c != ClaseCelda.Fuera)
            .Where(id => bbox is null || bbox.Intersecta(new Celda(id).Limites()))
            .ToList();
    }

    /// <summary>Bbox mínimo que contiene todas las celdas del corredor; (0,0,0,0) si no hay celdas.</summary>
    internal (double MinLon, double MinLat, double MaxLon, double MaxLat) BboxCorredor()
    {
        var celdas = Celdas;
        if (celdas.Count == 0) return (0, 0, 0, 0);

        double minLat = double.MaxValue, minLon = double.MaxValue;
        double maxLat = double.MinValue, maxLon = double.MinValue;
        foreach (var id in celdas)
        {
            var (mnLat, mnLon, mxLat, mxLon) = new Celda(id).Limites();
            if (mnLat < minLat) minLat = mnLat;
            if (mnLon < minLon) minLon = mnLon;
            if (mxLat > maxLat) maxLat = mxLat;
            if (mxLon > maxLon) maxLon = mxLon;
        }
        return (minLon, minLat, maxLon, maxLat);
    }
}
