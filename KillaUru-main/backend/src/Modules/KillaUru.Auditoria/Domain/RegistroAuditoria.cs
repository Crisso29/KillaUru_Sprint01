using System.Text.Json;
using KillaUru.Shared.Domain;
namespace KillaUru.Auditoria.Domain;

/// <summary>Hecho inmutable; la fábrica valida antes de persistir.</summary>
internal sealed class RegistroAuditoria
{
    public long Id { get; private set; }
    public DateTimeOffset OcurridoUtc { get; private set; }
    public Guid? ActorId { get; private set; }
    public string? ActorRol { get; private set; }
    public short Accion { get; private set; }
    public string? Objetivo { get; private set; }
    public string Contexto { get; private set; } = "{}";

    internal static ResultadoRegistro Crear(short accion, Guid? actorId, string? actorRol,
        string? objetivo, IReadOnlyDictionary<string, object?> contexto, DateTimeOffset ahora)
    {
        if (CatalogoAcciones.Categoria(accion) is null) return new(null, "accion_desconocida");
        if (objetivo?.Length > 100) return new(null, "objetivo_invalido");
        if ((actorId is null) != (actorRol is null) || actorId == Guid.Empty
            || actorRol is not (null or Roles.Administrador or Roles.ConductorRegistrado))
        {
            return new(null, "actor_invalido");
        }
        var copia = JsonSerializer.SerializeToElement(contexto);
        if (ClavesProhibidas.Contiene(copia)) return new(null, "contexto_no_permitido");
        // El JSON es una instantánea: cambios posteriores al diccionario no alteran el hecho registrado.
        return new(new RegistroAuditoria
        {
            Accion = accion, ActorId = actorId, ActorRol = actorRol, Objetivo = objetivo,
            Contexto = copia.GetRawText(), OcurridoUtc = ahora.ToUniversalTime(),
        }, null);
    }
}
