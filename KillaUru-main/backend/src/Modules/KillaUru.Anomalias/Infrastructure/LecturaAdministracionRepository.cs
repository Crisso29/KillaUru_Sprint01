using KillaUru.Anomalias.Application.Administracion;
using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Domain;
using Microsoft.EntityFrameworkCore;
namespace KillaUru.Anomalias.Infrastructure;
/// <summary>Conteos correlacionados en SQL, sin cargar reportes ni hacer una consulta por anomalía.</summary>
internal sealed class LecturaAdministracionRepository(AnomaliasDbContext db) : ILecturaAdministracionRepository
{
    public async Task<IReadOnlyList<ElementoPorRevisar>> PorRevisarAsync(CancellationToken ct)
    {
        var filas = await db.Anomalias.AsNoTracking().Where(a => a.PorRevisar).OrderByDescending(a => a.ActualizadaUtc)
            .Select(a => new { A = a, Reportes = db.Reportes.Count(r => r.AnomaliaId == a.Id) }).ToListAsync(ct);
        return filas.Select(x => new ElementoPorRevisar(x.A.Id, x.A.Tipo.ToString(), x.A.KmVia, x.A.SeveridadMostrada.Valor,
            x.A.MarcasRevision.Count, x.Reportes, x.A.UltimaDeteccionUtc)).ToArray();
    }
    public async Task<IReadOnlyList<ElementoPendiente>> PendientesAsync(CancellationToken ct)
    {
        var filas = await db.Anomalias.AsNoTracking().Where(a => a.Estado == EstadoAnomalia.PendienteConfirmacion).OrderBy(a => a.VenceUtc)
            .Select(a => new { A = a, Reportes = db.Reportes.Count(r => r.AnomaliaId == a.Id),
                Primero = db.Reportes.Where(r => r.AnomaliaId == a.Id).Min(r => (DateTimeOffset?)r.DetectadoUtc) }).ToListAsync(ct);
        return filas.Select(x => new ElementoPendiente(x.A.Id, x.A.Tipo.ToString(), x.A.KmVia, x.A.SeveridadMostrada.Valor,
            x.Reportes, x.Primero ?? x.A.UltimaDeteccionUtc, x.A.VenceUtc)).ToArray();
    }
    public async Task<IReadOnlyList<TransicionReciente>> TransicionesAsync(int limite, CancellationToken ct)
    {
        var filas = await (from t in db.Transiciones.AsNoTracking() join a in db.Anomalias on t.AnomaliaId equals a.Id
            orderby t.OcurridaUtc descending, t.Id descending select new { T = t, a.Tipo, a.KmVia }).Take(limite).ToListAsync(ct);
        return filas.Select(x => new TransicionReciente(x.T.AnomaliaId, x.Tipo.ToString(), x.KmVia, x.T.Desde?.ToString(),
            x.T.Hacia.ToString(), x.T.OcurridaUtc, x.T.ActorId)).ToArray();
    }
}
