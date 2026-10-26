using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Domain;
namespace KillaUru.Identidad.Application.Usuarios;

/// <summary>Valida invariantes y revoca todas las sesiones antes de confirmar el nuevo rol.</summary>
internal sealed class CambiarRolUseCase(IUsuarioRepositorio usuarios, ISesionRepositorio sesiones, ITransaccionIdentidad transaccion,
    ProteccionAdministradores proteccion, AuditarIdentidad auditoria, TimeProvider reloj)
{
    internal Task<ResultadoGestion> EjecutarAsync(Guid id, string rol, ActorIdentidad actor, CancellationToken ct)
    {
        if (rol is not ("Administrador" or "ConductorRegistrado")) return Task.FromResult(new ResultadoGestion("datos_invalidos", 400));
        return transaccion.EjecutarAsync(async () =>
        {
            var usuario = await usuarios.PorIdAsync(id, ct);
            if (usuario is null) return new ResultadoGestion("no_encontrado", 404);
            var error = rol == "ConductorRegistrado" ? await proteccion.ValidarRetiroAsync(usuario, actor.Id, ct) : null;
            if (error is not null) return new ResultadoGestion(error, 409);
            usuario.CambiarRol(Enum.Parse<RolUsuario>(rol));
            await sesiones.RevocarUsuarioAsync(id, reloj.GetUtcNow(), ct);
            await auditoria.EscrituraAsync(AccionAuditada.RolCambiado, actor, id, new Dictionary<string, object?> { ["rol"] = rol }, ct);
            return new ResultadoGestion();
        }, ct, protegerAdministradores: true);
    }
}
