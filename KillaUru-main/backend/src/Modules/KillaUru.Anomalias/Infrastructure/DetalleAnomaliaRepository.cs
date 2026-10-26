using KillaUru.Anomalias.Application.Administracion;
using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace KillaUru.Anomalias.Infrastructure;
/// <summary>Agrega conteos por día en SQL y excluye los seudónimos de las proyecciones.</summary>
internal sealed class DetalleAnomaliaRepository(AnomaliasDbContext db, IOptions<AnomaliasOptions> opciones) : IDetalleAnomaliaRepository
{
    public async Task<DetalleAnomalia?> ObtenerAsync(Guid id, CancellationToken ct)
    {
        var a = await db.Anomalias.AsNoTracking().SingleOrDefaultAsync(a => a.Id == id, ct);
        if (a is null) return null;
        var desde = a.UltimaDeteccionUtc.AddDays(-opciones.Value.VentanaSeveridadDias);
        var dias = await DiasAsync(id, desde, a.UltimaDeteccionUtc, ct);
        var historia = await db.Transiciones.AsNoTracking().Where(t => t.AnomaliaId == id).OrderBy(t => t.OcurridaUtc).ThenBy(t => t.Id).ToListAsync(ct);
        return new(a.Id, a.Tipo.ToString(), a.SeveridadMostrada.Valor, a.Estado.ToString(), a.Persistencia.ToString(),
            a.Posicion.Latitud, a.Posicion.Longitud, a.KmVia, dias.Count, await db.Reportes.CountAsync(r => r.AnomaliaId == id, ct),
            a.UltimaDeteccionUtc, a.Estado is EstadoAnomalia.Activa or EstadoAnomalia.PendienteConfirmacion ? a.VenceUtc : null,
            a.Estado == EstadoAnomalia.Activa, a.PorRevisar, a.MarcasRevision.Count, dias,
            historia.Select(t => new EstadoHistorico(t.Desde?.ToString(), t.Hacia.ToString(), t.OcurridaUtc, t.ActorId)).ToArray());
    }
    private async Task<List<ConteoDia>> DiasAsync(Guid id, DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct) =>
        await db.Database.SqlQuery<ConteoDia>($"""
            SELECT (detectado_utc AT TIME ZONE 'America/Lima')::date AS "Dia", count(*)::int AS "Cantidad"
            FROM anomalias.reporte_anomalia WHERE anomalia_id={id} AND detectado_utc >= {desde} AND detectado_utc <= {hasta}
            GROUP BY 1 ORDER BY 1
            """).ToListAsync(ct);
}
