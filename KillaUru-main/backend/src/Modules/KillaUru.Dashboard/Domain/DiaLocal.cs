namespace KillaUru.Dashboard.Domain;

internal static class DiaLocal
{
    internal static DateOnly Hoy(DateTimeOffset ahora, int desfaseHoras)
    {
        var local = ahora.ToOffset(TimeSpan.FromHours(desfaseHoras));
        return DateOnly.FromDateTime(local.DateTime);
    }

    internal static IReadOnlyList<DateOnly> DiasFaltantes(
        IReadOnlySet<DateOnly> guardados, DateOnly hoy, int recuperables)
    {
        var resultado = new List<DateOnly>();
        for (int i = recuperables; i >= 1; i--)
        {
            var dia = hoy.AddDays(-i);
            if (!guardados.Contains(dia)) resultado.Add(dia);
        }
        return resultado;
    }
}
