using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Domain;
using Microsoft.EntityFrameworkCore;
namespace KillaUru.Anomalias.Infrastructure;
/// <summary>Consultas indexadas de idempotencia y ventanas de detección.</summary>
internal sealed class ReporteRepositoryPostgis(AnomaliasDbContext db) : IReporteRepository
{
    public Task<bool> ExisteAsync(Guid eventoId, CancellationToken ct) => db.Reportes.AnyAsync(x => x.EventoId == eventoId, ct);
    public async Task<IReadOnlyList<ReporteAnomalia>> DesdeAsync(Guid anomalia, DateTimeOffset desde, CancellationToken ct) =>
        await db.Reportes.AsNoTracking().Where(x => x.AnomaliaId == anomalia && x.DetectadoUtc >= desde).ToListAsync(ct);
    public Task<ReporteAnomalia?> AnteriorAsync(Guid seudonimo, DateTimeOffset detectado, CancellationToken ct) =>
        db.Reportes.AsNoTracking().Where(x => x.Seudonimo == seudonimo && x.DetectadoUtc < detectado)
            .OrderByDescending(x => x.DetectadoUtc).ThenBy(x => x.EventoId).FirstOrDefaultAsync(ct);
    public void Agregar(ReporteAnomalia reporte, Guid anomalia) => db.Reportes.Add(reporte with { AnomaliaId = anomalia });
}
