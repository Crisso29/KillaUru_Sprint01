namespace KillaUru.Shared.Domain;

/// <summary>Severidad de un reporte o de una anomalía, de 1 a 5 (RN-02).</summary>
public readonly record struct Severidad
{
    private const int Minima = 1;
    private const int Maxima = 5;
    private const int AltaDesde = 4;

    private Severidad(int valor) => Valor = valor;

    /// <summary>Valor entre 1 y 5.</summary>
    public int Valor { get; }

    /// <summary>Clasificación fija: devuelve true para valores 4 o 5.</summary>
    /// <remarks>
    /// No consulta AnomaliasOptions.SeveridadAltaDesde. La política de confirmación de 004 debe usar
    /// ese parámetro para respetar la calibración de RN-06 sin recompilar (constitución III).
    /// </remarks>
    public bool EsAlta => Valor >= AltaDesde;

    /// <summary>Crea la severidad, o devuelve <c>null</c> si está fuera de 1 a 5.</summary>
    /// <param name="valor">Valor recibido.</param>
    /// <returns>La severidad, o <c>null</c>.</returns>
    public static Severidad? Crear(int valor) => valor is >= Minima and <= Maxima ? new Severidad(valor) : null;

    /// <summary>
    /// Mediana de las severidades redondeada hacia arriba (RN-05): con un número par, el promedio
    /// de las dos del medio. Ante la duda, avisa de más y no de menos.
    /// </summary>
    /// <param name="valores">Severidades de la ventana; al menos una.</param>
    /// <returns>La severidad mostrada.</returns>
    /// <exception cref="ArgumentException">La colección está vacía; el llamador debe resolver ese caso antes.</exception>
    public static Severidad MedianaHaciaArriba(IEnumerable<Severidad> valores)
    {
        var orden = valores.Select(s => s.Valor).Order().ToArray();
        if (orden.Length == 0)
        {
            throw new ArgumentException("La mediana necesita al menos una severidad.", nameof(valores));
        }

        var medio = orden.Length / 2;
        var mediana = orden.Length % 2 == 1 ? orden[medio] : (orden[medio - 1] + orden[medio]) / 2.0;
        return new Severidad((int)Math.Ceiling(mediana));
    }
}
