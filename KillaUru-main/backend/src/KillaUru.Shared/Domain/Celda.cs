namespace KillaUru.Shared.Domain;

/// <summary>
/// Celda de una rejilla global de 0,005° (≈ 550 m, ≥ 5 × el radio de agrupación de 100 m).
/// Se usa para el bloqueo por zona (ADR-005) y para la caché del mapa (ADR-006).
/// </summary>
/// <param name="Id">Identificador de la celda, tal como se guarda en <c>anomalia.celda</c>.</param>
public readonly record struct Celda(long Id)
{
    private const double Paso = 0.005;
    private const long DesplazamientoLatitud = 18_000;
    private const long DesplazamientoLongitud = 36_000;
    private const long Base = 100_000;
    private const double MetrosPorGrado = 100_000.0;   // menos que los ~111 km reales: da holgura al radio

    /// <summary>Celda que contiene la coordenada.</summary>
    /// <param name="coordenada">Punto a ubicar.</param>
    /// <returns>Su celda.</returns>
    public static Celda De(Coordenada coordenada) => new(IdDe(coordenada.Latitud, coordenada.Longitud));

    /// <summary>
    /// Celdas de las cuatro esquinas de un cuadro aproximado de ±<paramref name="metros"/> alrededor del punto,
    /// sin repetir y en orden ascendente para bloquearlas siempre en el mismo orden (sin interbloqueos).
    /// </summary>
    /// <remarks>
    /// Pensado para el radio de 100 m del corredor PE-28A: el cuadro cabe en dos filas y dos columnas.
    /// No enumera celdas intermedias para radios mayores ni calcula distancias geodésicas globales.
    /// </remarks>
    /// <param name="coordenada">Centro del cuadro.</param>
    /// <param name="metros">Medio lado del cuadro.</param>
    /// <returns>Las celdas, en orden ascendente.</returns>
    public static IReadOnlyList<Celda> Alrededor(Coordenada coordenada, double metros)
    {
        var d = metros / MetrosPorGrado;
        double[] desplazamientos = [-d, d];
        return desplazamientos
            .SelectMany(dLat => desplazamientos.Select(dLon =>
                new Celda(IdDe(Acotar(coordenada.Latitud + dLat, 90), Acotar(coordenada.Longitud + dLon, 180)))))
            .Distinct()
            .OrderBy(c => c.Id)
            .ToArray();
    }

    /// <summary>Rectángulo de la celda; es el inverso de <see cref="De"/>.</summary>
    /// <returns>Límites en grados.</returns>
    public (double MinLatitud, double MinLongitud, double MaxLatitud, double MaxLongitud) Limites()
    {
        var indiceLatitud = (Id / Base) - DesplazamientoLatitud;
        var indiceLongitud = (Id % Base) - DesplazamientoLongitud;
        return (indiceLatitud * Paso, indiceLongitud * Paso, (indiceLatitud + 1) * Paso, (indiceLongitud + 1) * Paso);
    }

    private static long IdDe(double latitud, double longitud) =>
        (((long)Math.Floor(latitud / Paso) + DesplazamientoLatitud) * Base) + (long)Math.Floor(longitud / Paso) + DesplazamientoLongitud;

    private static double Acotar(double valor, double limite) => Math.Clamp(valor, -limite, limite);
}
