namespace KillaUru.Shared.Domain;

/// <summary>Confianza del modelo en una detección, entre 0 y 1 (RN-04).</summary>
public readonly record struct Confidence
{
    private Confidence(double valor) => Valor = valor;

    /// <summary>Valor entre 0 y 1.</summary>
    public double Valor { get; }

    /// <summary>Crea la confianza, o devuelve <c>null</c> si está fuera de 0 a 1 o no es un número.</summary>
    /// <param name="valor">Valor recibido.</param>
    /// <returns>La confianza, o <c>null</c>.</returns>
    public static Confidence? Crear(double valor) => valor is >= 0.0 and <= 1.0 ? new Confidence(valor) : null;
}
