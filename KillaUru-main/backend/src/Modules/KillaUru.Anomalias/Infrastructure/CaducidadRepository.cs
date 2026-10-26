using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Application.Tareas;
using KillaUru.Anomalias.Contracts;
using KillaUru.Shared.Domain;
using Microsoft.EntityFrameworkCore;
namespace KillaUru.Anomalias.Infrastructure;
/// <summary>Estado e historial cambian atómicamente; SKIP LOCKED evita competir con trabajadores.</summary>
internal sealed class CaducidadRepository(AnomaliasDbContext db) : ICaducidadRepository
{
    public async Task<IReadOnlyList<AnomaliaCambiada>> CaducarAsync(DateTimeOffset ahora, CancellationToken ct)
    {
        var filas = await db.Database.SqlQuery<CaducidadFila>($"""
            WITH vencidas AS (SELECT id, estado, vence_utc FROM anomalias.anomalia
              WHERE estado IN (1,2) AND vence_utc <= {ahora} ORDER BY id LIMIT {LoteMantenimiento.Filas} FOR UPDATE SKIP LOCKED),
            cambios AS (UPDATE anomalias.anomalia a SET estado=3, estado_desde_utc=v.vence_utc, actualizada_utc={ahora}
              FROM vencidas v WHERE a.id=v.id RETURNING a.id, a.celda),
            historial AS (INSERT INTO anomalias.transicion(anomalia_id,desde,hacia,ocurrida_utc)
              SELECT id,estado,3,vence_utc FROM vencidas RETURNING anomalia_id)
            SELECT id AS "Id", celda AS "Celda" FROM cambios
            """).ToListAsync(ct);
        return filas.Select(f => new AnomaliaCambiada(f.Id, new Celda(f.Celda))).ToArray();
    }
}
