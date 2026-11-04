using System.Globalization;

namespace KillaUru.Mapa.Domain;

/// <summary>Rectángulo de consulta en «minLon,minLat,maxLon,maxLat» (R-003).</summary>
internal sealed record Bbox(double MinLon, double MinLat, double MaxLon, double MaxLat)
{
    /// <summary>
    /// Parsea la cadena "minLon,minLat,maxLon,maxLat".
    /// Devuelve <c>true</c> con resultado <c>null</c> para entrada vacía (sin filtro).
    /// Devuelve <c>false</c> para formato inválido, rango incorrecto o valores invertidos.
    /// </summary>
    internal static bool TryParse(string? entrada, out Bbox? resultado)
    {
        resultado = null;
        if (string.IsNullOrWhiteSpace(entrada)) { return true; }

        var partes = entrada.Split(',');
        if (partes.Length != 4) { return false; }

        if (!double.TryParse(partes[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var minLon)
         || !double.TryParse(partes[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var minLat)
         || !double.TryParse(partes[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var maxLon)
         || !double.TryParse(partes[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var maxLat))
        {
            return false;
        }

        if (minLon > maxLon || minLat > maxLat) { return false; }
        if (minLon < -180 || maxLon > 180 || minLat < -90 || maxLat > 90) { return false; }

        resultado = new Bbox(minLon, minLat, maxLon, maxLat);
        return true;
    }

    /// <summary>Devuelve <c>true</c> si este bbox se superpone con el rectángulo de celda dado.</summary>
    internal bool Intersecta((double MinLatitud, double MinLongitud, double MaxLatitud, double MaxLongitud) r) =>
        MinLon <= r.MaxLongitud && MaxLon >= r.MinLongitud &&
        MinLat <= r.MaxLatitud  && MaxLat >= r.MinLatitud;

    /// <summary>Devuelve <c>true</c> si el punto (lat, lon) está dentro de este bbox.</summary>
    internal bool Contiene(double lat, double lon) =>
        lat >= MinLat && lat <= MaxLat && lon >= MinLon && lon <= MaxLon;
}
