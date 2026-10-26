namespace KillaUru.Identidad.Domain;

/// <summary>Vencimiento deslizante del conductor y absoluto del administrador.</summary>
internal static class PoliticaSesion
{
    internal static DateTimeOffset Vencimiento(DateTimeOffset ahora, DateTimeOffset? limite, int dias, int horas)
        => HorasGuardadas.ALaHora(limite is null ? ahora.AddDays(dias) :
            (ahora.AddHours(horas) < limite.Value ? ahora.AddHours(horas) : limite.Value));
}
