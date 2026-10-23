using KillaUru.Auditoria.Domain;
namespace KillaUru.Auditoria.Application.Puertos;

/// <summary>Puerto de escritura append-only; no ofrece modificar ni borrar registros.</summary>
internal interface IRegistroAuditoriaRepositorio
{
    Task AgregarEnTransaccionAsync(RegistroAuditoria registro, CancellationToken ct);
    Task AgregarConTransaccionPropiaAsync(RegistroAuditoria registro, CancellationToken ct);
}
