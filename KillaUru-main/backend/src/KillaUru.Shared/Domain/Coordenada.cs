namespace KillaUru.Shared.Domain;

/// <summary>Punto WGS84 cuya fábrica <see cref="Crear"/> valida los rangos de latitud y longitud.</summary>
/// <remarks>Como value type, su valor predeterminado es (0, 0), una coordenada válida.</remarks>
public readonly record struct Coordenada
{
    private Coordenada(double latitud, double longitud)
    {
        Latitud = latitud;
        Longitud = longitud;
    }

    /// <summary>Latitud en grados, entre -90 y 90.</summary>
    public double Latitud { get; }

    /// <summary>Longitud en grados, entre -180 y 180.</summary>
    public double Longitud { get; }

    /// <summary>Crea la coordenada, o devuelve <c>null</c> si está fuera de rango o no es un número.</summary>
    /// <param name="latitud">Grados de latitud.</param>
    /// <param name="longitud">Grados de longitud.</param>
    /// <returns>La coordenada, o <c>null</c>.</returns>
    public static Coordenada? Crear(double latitud, double longitud) =>
        latitud is >= -90 and <= 90 && longitud is >= -180 and <= 180
            ? new Coordenada(latitud, longitud)
            : null;
}
