using KillaUru.Ingesta.Application.Puertos;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace KillaUru.Ingesta.Infrastructure;

/// <summary>Purgas de la tabla evento_recibido (data-model §3, FR-017, FR-018).</summary>
internal sealed class PurgasRecepcionPostgres(IngestaDbContext db) : IPurgasRecepcion
{
    public async Task<int> PurgarSeudonimosAsync(DateTimeOffset antesDe, CancellationToken ct)
    {
        // UPDATE en un solo UPDATE … IN (SELECT … LIMIT 5000) usando ix_recibido_purga
        return await db.Database.ExecuteSqlRawAsync(
            """
            UPDATE ingesta.evento_recibido
            SET seudonimo = NULL, lote_id = NULL, recibido_utc = date_trunc('hour', recibido_utc)
            WHERE evento_id IN (
                SELECT evento_id FROM ingesta.evento_recibido
                WHERE seudonimo IS NOT NULL AND recibido_utc < {0}
                LIMIT 5000
            )
            """,
            [antesDe], ct);
    }

    public async Task<int> BorrarFinalesAsync(DateTimeOffset antesDe, CancellationToken ct)
    {
        return await db.Database.ExecuteSqlRawAsync(
            """
            DELETE FROM ingesta.evento_recibido
            WHERE evento_id IN (
                SELECT evento_id FROM ingesta.evento_recibido
                WHERE estado IN (1, 2) AND recibido_utc < {0}
                LIMIT 5000
            )
            """,
            [antesDe], ct);
    }
}
