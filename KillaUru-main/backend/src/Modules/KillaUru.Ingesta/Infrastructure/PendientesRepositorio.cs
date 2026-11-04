using KillaUru.Anomalias.Contracts;
using KillaUru.Ingesta.Application.Procesamiento;
using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Ingesta.Domain;
using Microsoft.EntityFrameworkCore;

namespace KillaUru.Ingesta.Infrastructure;

/// <summary>Acceso a las filas Recibido para el trabajador: carga, conteo y marcado (FR-013, FR-014).</summary>
internal sealed class PendientesRepositorio(IngestaDbContext db) : IPendientesRepositorio
{
    public async Task<IReadOnlyList<EventoPendiente>> RecibidosDelLoteAsync(Guid? loteId, CancellationToken ct)
    {
        var filas = loteId.HasValue
            ? await db.Set<EventoRecibidoFila>()
                .Where(f => f.LoteId == loteId && f.Estado == EstadoRecepcion.Recibido)
                .OrderBy(f => f.DetectadoUtc).ThenBy(f => f.EventoId)
                .ToListAsync(ct)
            : await db.Set<EventoRecibidoFila>()
                .Where(f => f.LoteId == null && f.Estado == EstadoRecepcion.Recibido)
                .OrderBy(f => f.DetectadoUtc).ThenBy(f => f.EventoId)
                .ToListAsync(ct);

        return filas.Select(MapearPendiente).ToList();
    }

    public async Task<int> ContarEnLaHoraAsync(Guid seudonimo, DateTimeOffset inicioHora, CancellationToken ct)
    {
        var fin = inicioHora.AddHours(1);
        return await db.Set<EventoRecibidoFila>()
            .CountAsync(f => f.Seudonimo == seudonimo
                          && f.DetectadoUtc >= inicioHora
                          && f.DetectadoUtc < fin, ct);
    }

    public async Task MarcarAsync(Guid eventoId, EstadoRecepcion estado, short? motivo, CancellationToken ct)
    {
        await db.Set<EventoRecibidoFila>()
            .Where(f => f.EventoId == eventoId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(f => f.Estado, estado)
                .SetProperty(f => f.Motivo, motivo), ct);
    }

    private static EventoPendiente MapearPendiente(EventoRecibidoFila fila)
    {
        var payload = System.Text.Json.JsonDocument.Parse(fila.Payload).RootElement;
        var evento = new EventoReportado(
            EventoId: fila.EventoId,
            DetectadoUtc: fila.DetectadoUtc,
            Latitud: payload.TryGetProperty("latitud", out var lat) && lat.TryGetDouble(out var latVal) ? latVal : null,
            Longitud: payload.TryGetProperty("longitud", out var lon) && lon.TryGetDouble(out var lonVal) ? lonVal : null,
            PrecisionMetros: payload.TryGetProperty("precisionMetros", out var pm) && pm.TryGetDouble(out var pmVal) ? pmVal : null,
            Tipo: payload.TryGetProperty("tipo", out var t) ? t.GetString() : null,
            Severidad: payload.TryGetProperty("severidad", out var sev) && sev.TryGetInt32(out var sevVal) ? sevVal : null,
            Confidence: payload.TryGetProperty("confidence", out var c) && c.TryGetDouble(out var cVal) ? cVal : null,
            ModeloVersion: payload.TryGetProperty("modeloVersion", out var mv) ? mv.GetString() : null,
            Seudonimo: fila.Seudonimo);
        return new EventoPendiente(evento, fila.ConSesion, fila.RecibidoUtc);
    }
}
