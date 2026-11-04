namespace KillaUru.Dashboard.Domain;

internal static class CalculadoraIndicadores
{
    internal static MedidaIndicador CalcularEventos(IReadOnlyList<DiaIndicadores?> dias)
    {
        var d = Ultimo(dias);
        var valor = d is null ? (decimal?)null : d.EventosRecibidos;
        var comp  = Promedio7(dias, x => (decimal?)x.EventosRecibidos);
        return new(valor, comp, PctRelativo(valor, comp), Serie7(dias, x => (decimal?)x.EventosRecibidos));
    }

    internal static MedidaIndicador CalcularConductores(IReadOnlyList<DiaIndicadores?> dias)
    {
        var d = Ultimo(dias);
        var valor = d is null ? (decimal?)null : d.ConductoresActivos;
        var comp  = Promedio7(dias, x => (decimal?)x.ConductoresActivos);
        return new(valor, comp, Diferencia(valor, comp), Serie7(dias, x => (decimal?)x.ConductoresActivos));
    }

    internal static MedidaIndicador CalcularDispositivos(IReadOnlyList<DiaIndicadores?> dias)
    {
        var d = Ultimo(dias);
        var valor = d is null ? (decimal?)null : d.DispositivosDistintos;
        var comp  = Promedio7(dias, x => (decimal?)x.DispositivosDistintos);
        return new(valor, comp, Diferencia(valor, comp), Serie7(dias, x => (decimal?)x.DispositivosDistintos));
    }

    internal static MedidaIndicador CalcularCobertura(IReadOnlyList<DiaIndicadores?> dias)
    {
        var valor = CoberturaDelDia(Ultimo(dias));
        var comp  = Promedio7(dias, x => CoberturaDelDia(x));
        return new(valor, comp, Diferencia(valor, comp), Serie7(dias, x => CoberturaDelDia(x)));
    }

    internal static MedidaIndicador CalcularConfirmacion(IReadOnlyList<DiaIndicadores?> dias)
    {
        var (sc,  sp)  = SumaBinaria(dias, 1, 7, x => (x.Confirmaciones, x.PendientesExpiradas));
        var (scc, spc) = SumaBinaria(dias, 8, 7, x => (x.Confirmaciones, x.PendientesExpiradas));
        var valor = sc + sp   > 0 ? (decimal?)Math.Round((decimal)sc  / (sc  + sp)  * 100, 1) : null;
        var comp  = scc + spc > 0 ? (decimal?)Math.Round((decimal)scc / (scc + spc) * 100, 1) : null;
        return new(valor, comp, Diferencia(valor, comp),
            Serie7(dias, x => x.Confirmaciones + x.PendientesExpiradas > 0
                ? (decimal?)Math.Round((decimal)x.Confirmaciones / (x.Confirmaciones + x.PendientesExpiradas) * 100, 1)
                : null));
    }

    internal static MedidaIndicador CalcularReaparicion(IReadOnlyList<DiaIndicadores?> dias)
    {
        var (re, ex)   = SumaBinaria(dias, 1, 7, x => (x.Renacimientos, x.Expiraciones));
        var (rec, exc) = SumaBinaria(dias, 8, 7, x => (x.Renacimientos, x.Expiraciones));
        var valor = ex  > 0 ? (decimal?)Math.Round((decimal)re  / ex  * 100, 1) : null;
        var comp  = exc > 0 ? (decimal?)Math.Round((decimal)rec / exc * 100, 1) : null;
        return new(valor, comp, Diferencia(valor, comp),
            Serie7(dias, x => x.Expiraciones > 0
                ? (decimal?)Math.Round((decimal)x.Renacimientos / x.Expiraciones * 100, 1)
                : null));
    }

    private static DiaIndicadores? Ultimo(IReadOnlyList<DiaIndicadores?> dias) =>
        dias.Count > 0 ? dias[^1] : null;

    private static decimal? CoberturaDelDia(DiaIndicadores? d) =>
        d is { TramosTotales: > 0 }
            ? Math.Round((decimal)d.TramosConReportes7d / d.TramosTotales * 100, 1)
            : null;

    private static decimal? Promedio7(IReadOnlyList<DiaIndicadores?> dias, Func<DiaIndicadores, decimal?> sel)
    {
        decimal suma = 0; int n = 0;
        for (int i = 2; i <= 8; i++)
        {
            if (dias.Count >= i && dias[^i] is { } d) { var v = sel(d); if (v is not null) { suma += v.Value; n++; } }
        }
        return n == 0 ? null : Math.Round(suma / n, 1);
    }

    private static decimal?[] Serie7(IReadOnlyList<DiaIndicadores?> dias, Func<DiaIndicadores, decimal?> sel)
    {
        var r = new decimal?[7];
        for (int k = 0; k < 7; k++)
        {
            int fe = 7 - k;
            r[k] = dias.Count >= fe && dias[^fe] is { } d ? sel(d) : null;
        }
        return r;
    }

    private static (long A, long B) SumaBinaria(IReadOnlyList<DiaIndicadores?> dias, int desde, int count,
        Func<DiaIndicadores, (long, long)> sel)
    {
        long sa = 0, sb = 0;
        for (int i = desde; i < desde + count; i++)
        {
            if (dias.Count >= i && dias[^i] is { } d) { var (a, b) = sel(d); sa += a; sb += b; }
        }
        return (sa, sb);
    }

    private static decimal? PctRelativo(decimal? v, decimal? c) =>
        v is null || c is null || c == 0 ? null : Math.Round((v.Value - c.Value) / c.Value * 100m, 1);

    private static decimal? Diferencia(decimal? v, decimal? c) =>
        v is null || c is null ? null : Math.Round(v.Value - c.Value, 1);
}
