namespace KillaUru.Identidad.Domain;

/// <summary>Reduce la precisión temporal persistida para impedir correlaciones con eventos (D3).</summary>
internal static class HorasGuardadas
{
    // PostgreSQL almacena microsegundos: comparar a esa precisión conserva el límite inclusivo de 60 s.
    internal static DateTimeOffset AlMicrosegundo(DateTimeOffset instante) =>
        new(instante.UtcTicks - instante.UtcTicks % TimeSpan.TicksPerMicrosecond, TimeSpan.Zero);
    internal static DateTimeOffset ALaHora(DateTimeOffset instante) =>
        new(instante.UtcTicks - instante.UtcTicks % TimeSpan.TicksPerHour, TimeSpan.Zero);

    internal static DateTimeOffset AlDia(DateTimeOffset instante)
    {
        var local = instante.ToOffset(TimeSpan.FromHours(-5));
        return new DateTimeOffset(local.Year, local.Month, local.Day, 0, 0, 0, local.Offset).ToUniversalTime();
    }
}
