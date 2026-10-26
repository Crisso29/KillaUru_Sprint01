using KillaUru.Identidad.Application.Resultados;
namespace KillaUru.Identidad.Application.Usuarios;

/// <summary>Reutiliza la consulta auditada para resolver actores; las cuentas eliminadas no aparecen.</summary>
internal sealed class ResolverCorreosUseCase(ListarUsuariosUseCase listar)
{
    internal Task<PaginaUsuarios?> EjecutarAsync(Guid[] ids, ActorIdentidad actor, CancellationToken ct) =>
        listar.EjecutarAsync(new(Ids: ids, Tamano: 200), actor, ct);
}
