using System.Text.Json;
using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Ingesta.Application.Recepcion;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace KillaUru.Ingesta.Infrastructure;

/// <summary>
/// Inserta los eventos aceptados con un solo INSERT … ON CONFLICT (evento_id) DO NOTHING usando
/// unnest para enviar todos los arreglos en un único round-trip (ADR-005, FR-005; diseño-interno §6.4).
/// </summary>
internal sealed class RecepcionEventosPostgres(IngestaDbContext db) : IRecepcionEventos
{
    private const string Sql = """
        INSERT INTO ingesta.evento_recibido
            (evento_id, lote_id, recibido_utc, detectado_utc, seudonimo, con_sesion, estado, payload)
        SELECT
            u.evento_id, @loteId, @recibidoUtc, u.detectado_utc, @seudonimo, @conSesion, 0, u.payload
        FROM unnest(@eventoIds::uuid[], @detectados::timestamptz[], @payloads::jsonb[])
            AS u(evento_id, detectado_utc, payload)
        ON CONFLICT (evento_id) DO NOTHING
        RETURNING evento_id
        """;

    public async Task<IReadOnlySet<Guid>> InsertarAsync(LoteAceptado lote, CancellationToken ct)
    {
        var (loteId, recibidoUtc, conSesion, eventoIds, payloads, detectados) = lote.ComoParametros();
        if (eventoIds.Length == 0) return new HashSet<Guid>();

        var seudonimo = lote.Aceptados.Count > 0 ? lote.Aceptados[0].Seudonimo : (Guid?)null;
        var payloadsJson = payloads.Select(p => p.RootElement.GetRawText()).ToArray();

        var filas = await db.Database.SqlQueryRaw<Guid>(Sql,
            new Npgsql.NpgsqlParameter("loteId", loteId),
            new Npgsql.NpgsqlParameter("recibidoUtc", recibidoUtc),
            new Npgsql.NpgsqlParameter("seudonimo", seudonimo.HasValue ? seudonimo.Value : DBNull.Value),
            new Npgsql.NpgsqlParameter("conSesion", conSesion),
            new Npgsql.NpgsqlParameter("eventoIds", eventoIds) { NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid },
            new Npgsql.NpgsqlParameter("detectados", detectados) { NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.TimestampTz },
            new Npgsql.NpgsqlParameter("payloads", payloadsJson) { NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Jsonb })
            .ToListAsync(ct);
        return filas.ToHashSet();
    }
}
