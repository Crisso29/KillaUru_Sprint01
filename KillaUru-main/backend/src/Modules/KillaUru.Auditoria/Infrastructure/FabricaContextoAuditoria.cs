using KillaUru.Shared.Persistencia;
using Microsoft.EntityFrameworkCore;
namespace KillaUru.Auditoria.Infrastructure;

/// <summary>Crea contextos cortos; el contexto unido nunca es dueño de la conexión del llamador.</summary>
internal sealed class FabricaContextoAuditoria(DbContextOptions<AuditoriaDbContext> opciones, IContextoTransaccion transaccion)
{
    internal AuditoriaDbContext CrearPropio() => new(opciones);

    internal async Task<AuditoriaDbContext> CrearUnidoAsync(CancellationToken ct)
    {
        var conexion = transaccion.Conexion ?? throw new InvalidOperationException("Falta la conexión compartida.");
        var tx = transaccion.Transaccion ?? throw new InvalidOperationException("Falta la transacción compartida.");
        var configuracion = new DbContextOptionsBuilder<AuditoriaDbContext>().UseNpgsql(conexion).Options;
        var db = new AuditoriaDbContext(configuracion);
        try
        {
            await db.Database.UseTransactionAsync(tx, ct);
            return db;
        }
        catch
        {
            await db.DisposeAsync();
            throw;
        }
    }
}
