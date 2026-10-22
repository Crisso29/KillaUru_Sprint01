namespace KillaUru.Shared.Domain;

/// <summary>
/// Seudónimo rotativo de una instalación de la app (RF-27). No identifica a una persona;
/// el servidor lo borra a las 72 h de recibirlo (RN-09).
/// </summary>
public readonly record struct SeudonimoInstalacion
{
    private SeudonimoInstalacion(Guid valor) => Valor = valor;

    /// <summary>UUID aleatorio generado por la app.</summary>
    public Guid Valor { get; }

    /// <summary>Crea el seudónimo, o devuelve <c>null</c> si el texto no es un UUID o es el UUID vacío.</summary>
    /// <param name="texto">Texto recibido.</param>
    /// <returns>El seudónimo, o <c>null</c>.</returns>
    public static SeudonimoInstalacion? Crear(string? texto) =>
        Guid.TryParse(texto, out var valor) && valor != Guid.Empty ? new SeudonimoInstalacion(valor) : null;
}
