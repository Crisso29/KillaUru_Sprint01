namespace KillaUru.Identidad.Domain;

/// <summary>Reduce la precisión temporal persistida para impedir correlaciones con eventos (D3).</summary>
internal static class HorasGuardadas
{
    /// <summary>Perú no tiene horario de verano: UTC-5 fijo todo el año.</summary>
    internal static readonly TimeSpan DesfaseLima = TimeSpan.FromHours(-5);

    /// <summary>PostgreSQL almacena microsegundos: comparar a esa precisión conserva el límite inclusivo de 60 s.</summary>
    internal static DateTimeOffset AlMicrosegundo(DateTimeOffset instante) =>
        new(instante.UtcTicks - instante.UtcTicks % TimeSpan.TicksPerMicrosecond, TimeSpan.Zero);

    internal static DateTimeOffset ALaHora(DateTimeOffset instante) =>
        new(instante.UtcTicks - instante.UtcTicks % TimeSpan.TicksPerHour, TimeSpan.Zero);

    internal static DateTimeOffset AlDia(DateTimeOffset instante)
    {
        var enLima = instante.ToOffset(DesfaseLima);
        var medianocheEnLima = new DateTimeOffset(enLima.Year, enLima.Month, enLima.Day, 0, 0, 0, DesfaseLima);
        return medianocheEnLima.ToUniversalTime();
    }
}
