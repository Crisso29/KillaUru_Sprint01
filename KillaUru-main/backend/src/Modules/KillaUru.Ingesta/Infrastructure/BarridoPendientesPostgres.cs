using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Ingesta.Domain;
using Microsoft.EntityFrameworkCore;

namespace KillaUru.Ingesta.Infrastructure;

/// <summary>
/// Descubrimiento de lotes pendientes y diagnóstico de salud para el barredor y la comprobación
/// de salud (IBarridoPendientes, T-ARQ08: separado de PendientesRepositorio para respetar las 200 líneas).
/// </summary>
internal sealed class BarridoPendientesPostgres(IngestaDbContext db) : IBarridoPendientes
{
    public async Task<IReadOnlyList<Guid?>> LotesPendientesAsync(Guid? desde, int maximo, CancellationToken ct)
    {
        // Devuelve los loteId distintos (incluido null) de filas Recibido, paginados
        var query = db.Set<EventoRecibidoFila>()
            .Where(f => f.Estado == EstadoRecepcion.Recibido);

        if (desde.HasValue)
            query = query.Where(f => f.LoteId == null || f.LoteId.Value.CompareTo(desde.Value) > 0);

        var loteIds = await query
            .Select(f => f.LoteId)
            .Distinct()
            .OrderBy(id => id)
            .Take(maximo)
            .ToListAsync(ct);

        return loteIds;
    }

    public async Task<DateTimeOffset?> MasAntiguoPendienteAsync(CancellationToken ct)
    {
        return await db.Set<EventoRecibidoFila>()
            .Where(f => f.Estado == EstadoRecepcion.Recibido)
            .MinAsync(f => (DateTimeOffset?)f.RecibidoUtc, ct);
    }
}
