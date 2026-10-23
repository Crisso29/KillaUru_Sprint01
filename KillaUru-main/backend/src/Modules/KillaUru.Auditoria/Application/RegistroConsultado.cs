namespace KillaUru.Auditoria.Application;

/// <summary>Proyección de lectura: no devuelve una entidad EF ni su seguimiento de cambios.</summary>
internal sealed record RegistroConsultado(long Id, DateTimeOffset OcurridoUtc, Guid? ActorId,
    string? ActorRol, short Accion, string? Objetivo, string Contexto)
{
    internal string Categoria => Domain.CatalogoAcciones.Categoria(Accion)!;
    internal string NombreAccion => ((Contracts.AccionAuditada)Accion).ToString();
}
