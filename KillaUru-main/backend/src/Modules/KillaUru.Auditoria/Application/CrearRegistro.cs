using KillaUru.Auditoria.Contracts;
using KillaUru.Auditoria.Domain;
namespace KillaUru.Auditoria.Application;

/// <summary>Traduce el contrato al dominio; un dato interno inválido es un error del módulo llamador.</summary>
internal static class CrearRegistro
{
    internal static RegistroAuditoria De(AccionAuditada accion, ActorAuditoria actor, string? objetivo,
        IReadOnlyDictionary<string, object?> contexto, TimeProvider reloj)
    {
        var resultado = RegistroAuditoria.Crear((short)accion, actor.Id, actor.Rol, objetivo, contexto, reloj.GetUtcNow());
        return resultado.Registro ?? throw new InvalidOperationException($"Registro de auditoría inválido: {resultado.Error}.");
    }
}
