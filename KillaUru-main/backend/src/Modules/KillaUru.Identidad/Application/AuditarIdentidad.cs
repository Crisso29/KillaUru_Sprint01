using KillaUru.Auditoria.Contracts;

namespace KillaUru.Identidad.Application;

/// <summary>Contextos mínimos de auditoría: nunca copia correos, contraseñas ni valores libres del usuario.</summary>
internal sealed class AuditarIdentidad(IAuditoriaService auditoria)
{
    internal Task EscrituraAsync(AccionAuditada accion, ActorIdentidad? actor, Guid? objetivo,
        IReadOnlyDictionary<string, object?>? contexto, CancellationToken ct) =>
        auditoria.RegistrarAsync(accion, actor is null ? ActorAuditoria.Sistema : new ActorAuditoria(actor.Id, actor.Rol),
            objetivo?.ToString(), contexto ?? new Dictionary<string, object?>(), ct);
    internal Task LecturaAsync(ActorIdentidad actor, string objetivo, CancellationToken ct) =>
        auditoria.RegistrarLecturaAsync(AccionAuditada.AccesoAdministrativoDatos, new ActorAuditoria(actor.Id, actor.Rol),
            objetivo, new Dictionary<string, object?>(), ct);
}
