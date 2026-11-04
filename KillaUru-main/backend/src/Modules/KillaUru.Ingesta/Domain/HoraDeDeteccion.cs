namespace KillaUru.Ingesta.Domain;

/// <summary>
/// Calcula el bloque horario UTC [hh:00, hh:00+1h) de un instante dado.
/// Se usa para contar los eventos del mismo seudónimo en la hora del detector (RN-15a).
/// </summary>
internal static class HoraDeDeteccion
{
    /// <summary>Devuelve el inicio y el fin exclusivo del bloque horario UTC que contiene <paramref name="detectadoUtc"/>.</summary>
    internal static (DateTimeOffset Inicio, DateTimeOffset Fin) Bloque(DateTimeOffset detectadoUtc)
    {
        var utc = detectadoUtc.ToUniversalTime();
        var inicio = new DateTimeOffset(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, TimeSpan.Zero);
        return (inicio, inicio.AddHours(1));
    }
}
