using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Contracts.Lecturas;
using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Domain;
using Microsoft.EntityFrameworkCore;
namespace KillaUru.Anomalias.Infrastructure;
/// <summary>Filtra en PostgreSQL y materializa únicamente las proyecciones solicitadas.</summary>
internal sealed class LecturaAnomaliasRepository(AnomaliasDbContext db) : ILecturaAnomaliasRepository
{
    public async Task<IReadOnlyList<AnomaliaVisible>> ActivasAsync(IReadOnlyCollection<long> celdas, CancellationToken ct)
    {
        var claves = celdas.Select(c => new Celda(c)).ToArray();
        var filas = await db.Anomalias.AsNoTracking().Where(a => a.Estado == EstadoAnomalia.Activa && claves.Contains(a.Celda)).ToListAsync(ct);
        return filas.Select(a => new AnomaliaVisible(a.Id, (Contracts.TipoAnomalia)a.Tipo, a.SeveridadMostrada.Valor,
            a.Persistencia.ToString(), a.Posicion.Latitud, a.Posicion.Longitud, a.KmVia, a.Celda.Id, a.UltimaDeteccionUtc, a.VenceUtc)).ToArray();
    }
    public async Task<IReadOnlyList<AnomaliaCambio>> FiltrarAsync(FiltroAnomalias f, CancellationToken ct)
    {
        var estados = (f.Estados ?? [Contracts.EstadoAnomalia.Activa]).Select(e => (EstadoAnomalia)e).ToArray();
        var query = db.Anomalias.FromSqlInterpolated($"SELECT * FROM anomalias.anomalia WHERE ST_Covers(ST_MakeEnvelope({f.MinLon},{f.MinLat},{f.MaxLon},{f.MaxLat},4326),posicion::geometry)")
            .AsNoTracking().Where(a => estados.Contains(a.Estado));
        if (f.Tipo is not null) query = query.Where(a => a.Tipo == (TipoAnomalia)f.Tipo);
        if (f.DesdeUtc is not null) query = query.Where(a => a.UltimaDeteccionUtc >= f.DesdeUtc);
        if (f.HastaUtc is not null) query = query.Where(a => a.UltimaDeteccionUtc <= f.HastaUtc);
        if (f.Severidades is { Length: > 0 })
        {
            var niveles = f.Severidades.Select(s => Severidad.Crear(s) ?? throw new ArgumentOutOfRangeException(nameof(f))).ToArray();
            query = query.Where(a => niveles.Contains(a.SeveridadMostrada));
        }
        return (await query.OrderBy(a => a.Id).ToListAsync(ct)).Select(Mapear).ToArray();
    }
    public async Task<IReadOnlyList<AnomaliaCambio>> CambiosAsync(DateTimeOffset desde, CancellationToken ct) =>
        (await db.Anomalias.AsNoTracking().Where(a => a.ActualizadaUtc > desde).OrderBy(a => a.ActualizadaUtc).ThenBy(a => a.Id).ToListAsync(ct)).Select(Mapear).ToArray();
    private static AnomaliaCambio Mapear(Anomalia a) => new(a.Id, (Contracts.TipoAnomalia)a.Tipo, a.SeveridadMostrada.Valor,
        a.Persistencia.ToString(), a.Posicion.Latitud, a.Posicion.Longitud, a.KmVia, a.Celda.Id, a.UltimaDeteccionUtc,
        a.VenceUtc, (Contracts.EstadoAnomalia)a.Estado, a.ActualizadaUtc);
}
