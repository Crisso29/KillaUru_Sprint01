namespace KillaUru.Mapa.Domain;

/// <summary>Clasifica celdas del corredor respecto al polígono demo (RN-08; diseño-interno §4).</summary>
internal sealed class ZonaDemo(PoligonoGeo poligono)
{
    /// <summary>Dentro: todas las esquinas dentro. Borde: se superpone pero no completamente. Fuera: sin intersección.</summary>
    internal ClaseCelda Clasificar((double MinLatitud, double MinLongitud, double MaxLatitud, double MaxLongitud) r)
    {
        bool todasDentro =
            poligono.Contiene(r.MinLatitud, r.MinLongitud) &&
            poligono.Contiene(r.MinLatitud, r.MaxLongitud) &&
            poligono.Contiene(r.MaxLatitud, r.MinLongitud) &&
            poligono.Contiene(r.MaxLatitud, r.MaxLongitud);

        if (todasDentro) return ClaseCelda.Dentro;

        return poligono.Intersecta(r.MinLatitud, r.MinLongitud, r.MaxLatitud, r.MaxLongitud)
            ? ClaseCelda.Borde
            : ClaseCelda.Fuera;
    }

    /// <summary>Devuelve <c>true</c> si el punto está dentro del polígono demo.</summary>
    internal bool Contiene(double lat, double lon) => poligono.Contiene(lat, lon);
}
