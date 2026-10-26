namespace KillaUru.Identidad.Tests.Domain;

/// <summary>Instante fijo con minutos y segundos para que se note cualquier truncado (FR-039).</summary>
internal static class Instantes
{
    internal static readonly DateTimeOffset Ahora = new(2026, 10, 23, 10, 17, 43, TimeSpan.Zero);
}
