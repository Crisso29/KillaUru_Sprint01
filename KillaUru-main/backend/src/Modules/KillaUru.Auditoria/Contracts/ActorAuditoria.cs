namespace KillaUru.Auditoria.Contracts;

/// <summary>Identificador interno y rol; nunca incluye correo ni datos del dispositivo.</summary>
/// <param name="Id">Cuenta que actuó; null para una tarea del sistema.</param>
/// <param name="Rol">Nombre de rol de Shared.Domain.Roles; null para el sistema.</param>
public sealed record ActorAuditoria(Guid? Id, string? Rol)
{
    /// <summary>Actor de tareas automáticas, sin vínculo a una cuenta.</summary>
    public static readonly ActorAuditoria Sistema = new(null, null);
}
