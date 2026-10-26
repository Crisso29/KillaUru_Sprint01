using System.Data;
using System.Runtime.CompilerServices;
using KillaUru.Anomalias.Application.Exportacion;
using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Domain;
using Microsoft.EntityFrameworkCore;
namespace KillaUru.Anomalias.Infrastructure;
/// <summary>Snapshot de solo lectura; no se une a Auditoría, que confirma su propia transacción antes del stream.</summary>
internal sealed class ExportacionRepository(AnomaliasDbContext db) : IExportacionRepository
{
    public async Task<IAsyncDisposable> AbrirInstantaneaAsync(CancellationToken ct) =>
        await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
    public Task<int> ContarAsync(SolicitudExportacion filtros, CancellationToken ct) => Filtrar(filtros).CountAsync(ct);
    public async IAsyncEnumerable<FilaExportacion> LeerAsync(SolicitudExportacion filtros, [EnumeratorCancellation] CancellationToken ct)
    {
        var query = Filtrar(filtros).OrderBy(a => a.Id).Select(a => new { A = a,
            Cantidad = db.Reportes.Count(r => r.AnomaliaId == a.Id),
            Primero = db.Reportes.Where(r => r.AnomaliaId == a.Id).Min(r => (DateTimeOffset?)r.DetectadoUtc) });
        await foreach (var x in query.AsAsyncEnumerable().WithCancellation(ct))
        {
            yield return new(x.A.Id, x.A.Tipo.ToString(), x.A.SeveridadMostrada.Valor, x.A.Estado.ToString(), x.A.Persistencia.ToString(),
                x.A.KmVia, x.A.Posicion.Latitud, x.A.Posicion.Longitud, x.Cantidad, x.Primero ?? x.A.UltimaDeteccionUtc, x.A.UltimaDeteccionUtc);
        }
    }
    private IQueryable<Anomalia> Filtrar(SolicitudExportacion f)
    {
        var query = db.Anomalias.AsNoTracking();
        if (f.DesdeUtc is not null) query = query.Where(a => a.UltimaDeteccionUtc >= f.DesdeUtc);
        if (f.HastaUtc is not null) query = query.Where(a => a.UltimaDeteccionUtc <= f.HastaUtc);
        if (f.Tipo is not null) { var tipo = Enum.Parse<TipoAnomalia>(f.Tipo); query = query.Where(a => a.Tipo == tipo); }
        if (f.Estados is { Length: > 0 })
        {
            var estados = f.Estados.Select(Enum.Parse<EstadoAnomalia>).ToArray();
            query = query.Where(a => estados.Contains(a.Estado));
        }
        return query;
    }
}
