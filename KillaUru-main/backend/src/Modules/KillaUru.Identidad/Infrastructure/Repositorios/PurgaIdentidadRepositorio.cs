using KillaUru.Identidad.Application.Puertos;
using Microsoft.EntityFrameworkCore;
namespace KillaUru.Identidad.Infrastructure.Repositorios;

/// <summary>Procesa lotes acotados usando instantes del reloj recibido, nunca now() de PostgreSQL.</summary>
internal sealed class PurgaIdentidadRepositorio(IdentidadDbContext db) : IPurgaIdentidadRepositorio
{
    public async Task PurgarAsync(DateTimeOffset ahora, int dias, int minutos, CancellationToken ct)
    {
        var corte = ahora.AddDays(-dias);
        var uso = ahora.AddMinutes(-minutos);
        while (await db.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE FROM identidad.sesion_refresh WHERE id IN
            (SELECT id FROM identidad.sesion_refresh WHERE expira_utc < {corte} OR revocado_utc < {corte} LIMIT 5000)
            """, ct) > 0) { ct.ThrowIfCancellationRequested(); }
        while (await db.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE FROM identidad.token_correo WHERE hash_token IN
            (SELECT hash_token FROM identidad.token_correo WHERE expira_utc < {corte} OR usado_utc < {corte} LIMIT 5000)
            """, ct) > 0) { ct.ThrowIfCancellationRequested(); }
        await TruncarAsync(uso, ct);
    }

    private async Task TruncarAsync(DateTimeOffset corte, CancellationToken ct)
    {
        while (await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE identidad.sesion_refresh SET usado_utc = date_trunc('hour', usado_utc)
            WHERE id IN (SELECT id FROM identidad.sesion_refresh WHERE usado_utc < {corte}
            AND usado_utc <> date_trunc('hour', usado_utc) LIMIT 5000)
            """, ct) > 0) { ct.ThrowIfCancellationRequested(); }
    }
}
