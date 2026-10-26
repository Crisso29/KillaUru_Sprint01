using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Contracts.Lecturas;
using Microsoft.EntityFrameworkCore;
namespace KillaUru.Anomalias.Infrastructure;
/// <summary>Intervalos UTC semiabiertos del día de Lima; el truncado a horas conserva el día recibido.</summary>
internal sealed class ResumenDiaRepository(AnomaliasDbContext db) : IResumenDiaRepository
{
    public async Task<ResumenDia> ObtenerAsync(DateOnly dia, int tramosTotales, CancellationToken ct)
    {
        var inicio = new DateTimeOffset(dia, TimeOnly.MinValue, TimeSpan.FromHours(-5)).ToUniversalTime();
        var fin = inicio.AddDays(1); var semana = fin.AddDays(-7);
        var filas = await db.Database.SqlQuery<ResumenFila>($"""
            SELECT (SELECT count(*) FROM anomalias.reporte_anomalia WHERE recibido_utc >= {inicio} AND recibido_utc < {fin}) AS "Eventos",
              (SELECT count(DISTINCT seudonimo) FROM anomalias.reporte_anomalia WHERE recibido_utc >= {inicio} AND recibido_utc < {fin}) AS "Dispositivos",
              (SELECT count(DISTINCT floor(km_via))::int FROM anomalias.reporte_anomalia WHERE detectado_utc >= {semana} AND detectado_utc < {fin}) AS "Tramos",
              count(*) FILTER (WHERE desde=2 AND hacia=1) AS "Confirmaciones",
              count(*) FILTER (WHERE desde=2 AND hacia=3) AS "Pendientes",
              count(*) FILTER (WHERE hacia=3) AS "Expiraciones",
              count(*) FILTER (WHERE desde=3 AND hacia IN (1,2)) AS "Renacimientos"
            FROM anomalias.transicion WHERE ocurrida_utc >= {inicio} AND ocurrida_utc < {fin}
            """).ToListAsync(ct);
        var r = filas.Single();
        return new(dia, r.Eventos, r.Dispositivos, r.Tramos, tramosTotales, r.Confirmaciones, r.Pendientes, r.Expiraciones, r.Renacimientos);
    }
}
