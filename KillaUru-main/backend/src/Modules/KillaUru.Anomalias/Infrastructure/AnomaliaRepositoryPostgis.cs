using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Domain;
using Microsoft.EntityFrameworkCore;
namespace KillaUru.Anomalias.Infrastructure;
/// <summary>PostGIS agrupa por geografía real y bloquea la fila antes de modificar el agregado.</summary>
internal sealed class AnomaliaRepositoryPostgis(AnomaliasDbContext db) : IAnomaliaRepository
{
    public async Task<Anomalia?> MasCercanaAsync(Coordenada punto, TipoAnomalia tipo, double radio, CancellationToken ct)
    {
        var filas = await db.Anomalias.FromSqlInterpolated($"""
            SELECT * FROM anomalias.anomalia
            WHERE tipo = {(short)tipo} AND ST_DWithin(posicion, ST_SetSRID(ST_MakePoint({punto.Longitud}, {punto.Latitud}),4326)::geography, {radio})
            ORDER BY ST_Distance(posicion, ST_SetSRID(ST_MakePoint({punto.Longitud}, {punto.Latitud}),4326)::geography), id
            LIMIT 1 FOR UPDATE
            """).ToListAsync(ct);
        return filas.SingleOrDefault();
    }
    public async Task<Anomalia?> ObtenerParaActualizarAsync(Guid id, CancellationToken ct) =>
        (await db.Anomalias.FromSqlInterpolated($"SELECT * FROM anomalias.anomalia WHERE id = {id} FOR UPDATE").ToListAsync(ct)).SingleOrDefault();
    public Task<Anomalia?> ObtenerAsync(Guid id, CancellationToken ct) => db.Anomalias.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task GuardarAsync(Anomalia anomalia, bool nueva, CancellationToken ct)
    {
        if (nueva) db.Anomalias.Add(anomalia);
        db.Transiciones.AddRange(anomalia.Transiciones.Select(t => t with { AnomaliaId = anomalia.Id }));
        await db.SaveChangesAsync(ct);
        anomalia.Transiciones.Clear();
    }
}
