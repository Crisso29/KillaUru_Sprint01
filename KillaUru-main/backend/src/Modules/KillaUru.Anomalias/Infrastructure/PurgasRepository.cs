using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Application.Tareas;
using Microsoft.EntityFrameworkCore;
namespace KillaUru.Anomalias.Infrastructure;
/// <summary>Lotes acotados y bloqueos de fila: anonimización y truncado son una sola escritura.</summary>
internal sealed class PurgasRepository(AnomaliasDbContext db) : IPurgasRepository
{
    public Task<int> PurgarSeudonimosAsync(DateTimeOffset limite, CancellationToken ct) => db.Database.ExecuteSqlInterpolatedAsync($"""
        WITH lote AS (SELECT evento_id FROM anomalias.reporte_anomalia WHERE seudonimo IS NOT NULL
          AND recibido_utc < {limite} ORDER BY evento_id LIMIT {LoteMantenimiento.Filas} FOR UPDATE SKIP LOCKED)
        UPDATE anomalias.reporte_anomalia r SET seudonimo=NULL, recibido_utc=date_trunc('hour',r.recibido_utc AT TIME ZONE 'UTC') AT TIME ZONE 'UTC'
        FROM lote l WHERE r.evento_id=l.evento_id
        """, ct);

    public async Task<int> PurgarHistoricoAsync(DateTimeOffset limite, CancellationToken ct)
    {
        var reportes = await db.Database.ExecuteSqlInterpolatedAsync($"""
            WITH lote AS (SELECT evento_id FROM anomalias.reporte_anomalia WHERE detectado_utc < {limite}
              ORDER BY evento_id LIMIT {LoteMantenimiento.Filas} FOR UPDATE SKIP LOCKED)
            DELETE FROM anomalias.reporte_anomalia r USING lote l WHERE r.evento_id=l.evento_id
            """, ct);
        var anomalias = await db.Database.ExecuteSqlInterpolatedAsync($"""
            WITH lote AS (SELECT id FROM anomalias.anomalia WHERE estado_desde_utc < {limite}
              AND (estado IN (3,4) OR (estado=5 AND ultima_deteccion_utc < {limite}))
              ORDER BY id LIMIT {LoteMantenimiento.Filas} FOR UPDATE SKIP LOCKED)
            DELETE FROM anomalias.anomalia a USING lote l WHERE a.id=l.id
            """, ct);
        return Math.Max(reportes, anomalias);
    }
}
