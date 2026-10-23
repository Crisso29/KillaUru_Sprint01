namespace KillaUru.Shared.Tareas;

/// <summary>
/// Calcula el próximo instante de una tarea por hora de reloj (T-OPS05).
/// La hora local es la de Lima, UTC−5 fijo (Perú no tiene horario de verano).
/// </summary>
/// <remarks>
/// No ejecuta ni espera tareas. El servicio consumidor debe ejecutar el trabajo al arrancar y luego
/// esperar hasta el instante calculado, usando su TimeProvider y su token de cancelación.
/// </remarks>
public static class ProgramaReloj
{
    private static readonly TimeSpan DesfaseLocal = TimeSpan.FromHours(-5);

    /// <summary>Próximo instante con ese minuto, estrictamente después de <paramref name="ahora"/>.</summary>
    /// <param name="ahora">Instante actual (de <see cref="TimeProvider"/>).</param>
    /// <param name="minuto">Minuto de la hora, de 0 a 59.</param>
    /// <returns>El próximo instante, en UTC.</returns>
    public static DateTimeOffset ProximaHoraria(DateTimeOffset ahora, int minuto)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minuto);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minuto, 59);

        var utc = ahora.ToUniversalTime();
        var candidato = new DateTimeOffset(utc.Year, utc.Month, utc.Day, utc.Hour, minuto, 0, TimeSpan.Zero);
        return candidato > utc ? candidato : candidato.AddHours(1);
    }

    /// <summary>Próximo instante con esa hora de Lima, estrictamente después de <paramref name="ahora"/>.</summary>
    /// <param name="ahora">Instante actual (de <see cref="TimeProvider"/>).</param>
    /// <param name="horaLocal">Hora del día en Lima.</param>
    /// <returns>El próximo instante, en UTC.</returns>
    public static DateTimeOffset ProximaDiaria(DateTimeOffset ahora, TimeOnly horaLocal)
    {
        var local = ahora.ToOffset(DesfaseLocal);
        var candidato = new DateTimeOffset(DateOnly.FromDateTime(local.DateTime), horaLocal, DesfaseLocal);
        var proxima = candidato > ahora ? candidato : candidato.AddDays(1);
        return proxima.ToUniversalTime();
    }
}
