using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Application.Usuarios;
using KillaUru.Identidad.Domain;
namespace KillaUru.Identidad.Application.Privilegios;

/// <summary>Audita la consulta porque revela los correos de los administradores.</summary>
internal sealed class ConsultarRevisionUseCase(IUsuarioRepositorio usuarios, IRevisionRepositorio revisiones,
    AuditarIdentidad auditoria, TimeProvider reloj)
{
    internal async Task<EstadoRevision> EjecutarAsync(ActorIdentidad actor, CancellationToken ct)
    {
        var ultima = await revisiones.UltimaAsync(ct);
        var administradores = await usuarios.AdministradoresAsync(ct);
        await auditoria.LecturaAsync(actor, "identidad.revision_privilegios", ct);
        return new(ultima, RevisionPrivilegios.Vence(ultima, reloj.GetUtcNow()), administradores.Select(ProyectarUsuario.Vista).ToArray());
    }
}
