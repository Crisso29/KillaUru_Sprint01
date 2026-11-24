using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Domain;
using Microsoft.EntityFrameworkCore;
namespace KillaUru.Anomalias.Infrastructure;
/// <summary>Consultas indexadas de idempotencia y ventanas de detección.</summary>
internal sealed class ReporteRepositoryPostgis(AnomaliasDbContext db) : IReporteRepository
{
    public Task<bool> ExisteAsync(Guid eventoId, CancellationToken ct) => db.Reportes.AnyAsync(x => x.EventoId == eventoId, ct);
    public async Task<IReadOnlyList<ReporteAnomalia>> DesdeAsync(Guid anomalia, DateTimeOffset desde, CancellationToken ct)
    {
        // Solo los 5 campos que las reglas de dominio usan (DetectadoUtc, Severidad, Seudonimo,
        // ConSesion, EventoId). No se carga la geometría Posicion ni los demás campos (Fix B, DT-01).
        var filas = await db.Reportes.AsNoTracking()
            .Where(x => x.AnomaliaId == anomalia && x.DetectadoUtc >= desde)
            .Select(x => new { x.EventoId, x.DetectadoUtc, x.Severidad, x.Seudonimo, x.ConSesion })
            .ToListAsync(ct);
        return filas.ConvertAll(x =>
            new ReporteAnomalia(x.EventoId, default, x.DetectadoUtc, default, default, 0,
                x.Severidad, default, "", x.Seudonimo, x.ConSesion));
    }
    public Task<ReporteAnomalia?> AnteriorAsync(Guid seudonimo, DateTimeOffset detectado, CancellationToken ct) =>
        db.Reportes.AsNoTracking().Where(x => x.Seudonimo == seudonimo && x.DetectadoUtc < detectado)
            .OrderByDescending(x => x.DetectadoUtc).ThenBy(x => x.EventoId).FirstOrDefaultAsync(ct);
    public void Agregar(ReporteAnomalia reporte, Guid anomalia) => db.Reportes.Add(reporte with { AnomaliaId = anomalia });
}
