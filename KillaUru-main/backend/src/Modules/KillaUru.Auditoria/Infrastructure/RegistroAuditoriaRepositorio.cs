using KillaUru.Auditoria.Application.Puertos;
using KillaUru.Auditoria.Domain;
using Microsoft.EntityFrameworkCore;
namespace KillaUru.Auditoria.Infrastructure;

/// <summary>Escritura append-only: distingue compartir una transacción de confirmar una lectura.</summary>
internal sealed class RegistroAuditoriaRepositorio(FabricaContextoAuditoria fabrica) : IRegistroAuditoriaRepositorio
{
    public async Task AgregarEnTransaccionAsync(RegistroAuditoria registro, CancellationToken ct)
    {
        await using var db = await fabrica.CrearUnidoAsync(ct);
        db.Registros.Add(registro);
        await db.SaveChangesAsync(ct);
        // No confirmar aquí: la acción de negocio y este registro deben tener el mismo desenlace.
    }

    public async Task AgregarConTransaccionPropiaAsync(RegistroAuditoria registro, CancellationToken ct)
    {
        await using var db = fabrica.CrearPropio();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.Registros.Add(registro);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
