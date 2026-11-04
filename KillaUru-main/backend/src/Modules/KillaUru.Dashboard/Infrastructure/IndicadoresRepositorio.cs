using KillaUru.Dashboard.Application.Puertos;
using KillaUru.Dashboard.Domain;
using Microsoft.EntityFrameworkCore;

namespace KillaUru.Dashboard.Infrastructure;

internal sealed class IndicadoresRepositorio(DashboardDbContext ctx) : IIndicadoresRepositorio
{
    public async Task<IReadOnlySet<DateOnly>> DiasCompletosAsync(DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var completos = await ctx.IndicadoresDiarios
            .AsNoTracking()
            .Where(x => x.Dia >= desde && x.Dia <= hasta)
            .GroupBy(x => x.Dia)
            .Where(g => g.Count() == 9)
            .Select(g => g.Key)
            .ToListAsync(ct);
        return completos.ToHashSet();
    }

    public async Task GuardarDiaAsync(DiaIndicadores dia, CancellationToken ct)
    {
        await using var tx = await ctx.Database.BeginTransactionAsync(ct);
        foreach (var (ind, val) in dia.ComoFilas())
        {
            await ctx.Database.ExecuteSqlAsync(
                $"INSERT INTO dashboard.indicador_diario (dia, indicador, valor) VALUES ({dia.Dia}, {(short)ind}, {val}) ON CONFLICT (dia, indicador) DO NOTHING",
                ct);
        }
        await tx.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<DiaIndicadores?>> LeerAsync(DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var filas = await ctx.IndicadoresDiarios
            .AsNoTracking()
            .Where(x => x.Dia >= desde && x.Dia <= hasta)
            .ToListAsync(ct);
        var porDia = filas
            .GroupBy(x => x.Dia)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.Indicador, x => x.Valor));
        var resultado = new List<DiaIndicadores?>();
        for (var d = desde; d <= hasta; d = d.AddDays(1))
            resultado.Add(ConstruirDia(d, porDia.GetValueOrDefault(d)));
        return resultado;
    }

    public Task<int> PurgarAsync(DateOnly anteDe, CancellationToken ct) =>
        ctx.IndicadoresDiarios.Where(x => x.Dia < anteDe).ExecuteDeleteAsync(ct);

    private static DiaIndicadores? ConstruirDia(DateOnly dia, Dictionary<Indicador, decimal>? f)
    {
        if (f is null || f.Count < 9) return null;
        return f.TryGetValue(Indicador.EventosRecibidos,      out var ev)
            && f.TryGetValue(Indicador.ConductoresActivos,    out var ca)
            && f.TryGetValue(Indicador.DispositivosDistintos, out var dd)
            && f.TryGetValue(Indicador.TramosConReportes7d,   out var t7)
            && f.TryGetValue(Indicador.TramosTotales,         out var tt)
            && f.TryGetValue(Indicador.Confirmaciones,        out var conf)
            && f.TryGetValue(Indicador.PendientesExpiradas,   out var pend)
            && f.TryGetValue(Indicador.Expiraciones,          out var exp)
            && f.TryGetValue(Indicador.Renacimientos,         out var renac)
            ? new DiaIndicadores(dia, (long)ev, (long)ca, (long)dd, (int)t7, (int)tt,
                (long)conf, (long)pend, (long)exp, (long)renac)
            : null;
    }
}
