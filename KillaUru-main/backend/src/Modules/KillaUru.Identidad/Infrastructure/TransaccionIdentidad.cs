using KillaUru.Identidad.Application.Puertos;
using KillaUru.Shared.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace KillaUru.Identidad.Infrastructure;

/// <summary>Confirma por el contexto compartido para ejecutar los callbacks de Auditoría una sola vez.</summary>
internal sealed class TransaccionIdentidad(IdentidadDbContext db, IContextoTransaccion contexto) : ITransaccionIdentidad
{
    public async Task<T> EjecutarAsync<T>(Func<Task<T>> accion, CancellationToken ct, bool protegerAdministradores = false)
    {
        await using var transaccion = await db.Database.BeginTransactionAsync(ct);
        contexto.Unir(db.Database.GetDbConnection(), transaccion.GetDbTransaction());
        try
        {
            // Solo los cambios de privilegios se serializan globalmente; cada cuenta se protege por su fila.
            if (protegerAdministradores) await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(3003)", ct);
            var resultado = await accion();
            await db.SaveChangesAsync(ct);
            await contexto.ConfirmarAsync(ct);
            return resultado;
        }
        catch
        {
            if (contexto.Transaccion is not null) await contexto.RevertirAsync(CancellationToken.None);
            throw;
        }
    }

    public void AlConfirmar(Func<Task> accion) => contexto.AlConfirmar(accion);
}
