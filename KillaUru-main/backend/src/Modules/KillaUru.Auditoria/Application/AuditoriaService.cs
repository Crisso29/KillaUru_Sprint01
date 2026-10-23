using KillaUru.Auditoria.Contracts;
namespace KillaUru.Auditoria.Application;

/// <summary>Adaptador del contrato público a los dos casos de uso; no contiene reglas de negocio.</summary>
internal sealed class AuditoriaService(RegistrarAuditoriaUseCase escritura, RegistrarLecturaUseCase lectura) : IAuditoriaService
{
    public Task RegistrarAsync(AccionAuditada accion, ActorAuditoria actor, string? objetivo,
        IReadOnlyDictionary<string, object?> contexto, CancellationToken ct) =>
        escritura.EjecutarAsync(accion, actor, objetivo, contexto, ct);

    public Task RegistrarLecturaAsync(AccionAuditada accion, ActorAuditoria actor, string? objetivo,
        IReadOnlyDictionary<string, object?> contexto, CancellationToken ct) =>
        lectura.EjecutarAsync(accion, actor, objetivo, contexto, ct);
}
