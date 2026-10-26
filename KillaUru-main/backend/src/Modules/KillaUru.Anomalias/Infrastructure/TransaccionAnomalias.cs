using Npgsql;
using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Shared.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
namespace KillaUru.Anomalias.Infrastructure;
/// <summary>Orden global: evento, seudónimo y celdas ordenadas; comparte la transacción con Auditoría.</summary>
internal sealed class TransaccionAnomalias(AnomaliasDbContext db, IContextoTransaccion contexto) : ITransaccionAnomalias
{
    public async Task<T> EjecutarAsync<T>(IReadOnlyCollection<long> celdas, Guid? evento, Guid? seudonimo, Func<Task<T>> accion, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        contexto.Unir(db.Database.GetDbConnection(), tx.GetDbTransaction());
        try
        {
            await BloquearAsync(celdas, evento, seudonimo, ct);
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
    private async Task BloquearAsync(IReadOnlyCollection<long> celdas, Guid? evento, Guid? seudonimo, CancellationToken ct)
    {
        var claves = new List<string>();
        if (evento is not null) claves.Add("Aevento:" + evento);
        if (seudonimo is not null) claves.Add("Aseudonimo:" + seudonimo);
        claves.AddRange(celdas.Distinct().Order().Select(c => "Acelda:" + c));
        if (claves.Count == 0) return;
        // NpgsqlBatch conserva la secuencia de sentencias y elimina un viaje de red por bloqueo.
        await using var lote = new NpgsqlBatch((NpgsqlConnection)db.Database.GetDbConnection(),
            (NpgsqlTransaction)db.Database.CurrentTransaction!.GetDbTransaction());
        foreach (var clave in claves)
        {
            var comando = new NpgsqlBatchCommand("SELECT pg_advisory_xact_lock(hashtextextended($1, 0))");
            comando.Parameters.Add(new NpgsqlParameter { Value = clave });
            lote.BatchCommands.Add(comando);
        }
        await lote.ExecuteNonQueryAsync(ct);
    }
    public void AlConfirmar(Func<Task> accion) => contexto.AlConfirmar(accion);
}
