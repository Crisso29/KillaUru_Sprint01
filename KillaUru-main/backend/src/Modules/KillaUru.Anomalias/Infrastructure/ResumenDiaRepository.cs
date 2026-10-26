using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Contracts.Lecturas;
using KillaUru.Anomalias.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KillaUru.Anomalias.Infrastructure;

/// <summary>Intervalos UTC semiabiertos del día de Lima; el truncado a horas conserva el día recibido.</summary>
internal sealed class ResumenDiaRepository(AnomaliasDbContext db, IOptions<AnomaliasOptions> opciones) : IResumenDiaRepository
{
    public async Task<ResumenDia> ObtenerAsync(DateOnly dia, int tramosTotales, CancellationToken ct)
    {
        // El mismo desfase que usa la persistencia (RN-12): ambos cuentan los mismos días locales.
        var desfaseLocal = TimeSpan.FromHours(opciones.Value.DesfaseHorarioHoras);
        var inicio = new DateTimeOffset(dia, TimeOnly.MinValue, desfaseLocal).ToUniversalTime();
        var fin = inicio.AddDays(1);
        // Los tramos de 1 km con reportes se miden en los últimos 7 días, no solo en el día (spec 004, escenario US4-4).
        var inicioSemana = fin.AddDays(-7);
        var resumen = await ConsultarAsync(inicio, fin, inicioSemana, ct);
        return new(dia, resumen.Eventos, resumen.Dispositivos, resumen.Tramos, tramosTotales,
            resumen.Confirmaciones, resumen.Pendientes, resumen.Expiraciones, resumen.Renacimientos);
    }

    /// <summary>Una sola ida a la base: conteos de reportes y clasificación de las transiciones del día.</summary>
    private async Task<ResumenFila> ConsultarAsync(DateTimeOffset inicio, DateTimeOffset fin, DateTimeOffset inicioSemana, CancellationToken ct)
    {
        var filas = await db.Database.SqlQuery<ResumenFila>($"""
            SELECT (SELECT count(*) FROM anomalias.reporte_anomalia WHERE recibido_utc >= {inicio} AND recibido_utc < {fin}) AS "Eventos",
              (SELECT count(DISTINCT seudonimo) FROM anomalias.reporte_anomalia WHERE recibido_utc >= {inicio} AND recibido_utc < {fin}) AS "Dispositivos",
              (SELECT count(DISTINCT floor(km_via))::int FROM anomalias.reporte_anomalia WHERE detectado_utc >= {inicioSemana} AND detectado_utc < {fin}) AS "Tramos",
              count(*) FILTER (WHERE desde=2 AND hacia=1) AS "Confirmaciones",
              count(*) FILTER (WHERE desde=2 AND hacia=3) AS "Pendientes",
              count(*) FILTER (WHERE hacia=3) AS "Expiraciones",
              count(*) FILTER (WHERE desde=3 AND hacia IN (1,2)) AS "Renacimientos"
            FROM anomalias.transicion WHERE ocurrida_utc >= {inicio} AND ocurrida_utc < {fin}
            """).ToListAsync(ct);
        return filas.Single();
    }
}
