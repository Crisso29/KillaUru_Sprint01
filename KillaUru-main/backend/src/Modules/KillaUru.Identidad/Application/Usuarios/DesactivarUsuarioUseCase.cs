using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
namespace KillaUru.Identidad.Application.Usuarios;

/// <summary>El motivo se valida pero no se copia a auditoría: puede contener datos personales libres.</summary>
internal sealed class DesactivarUsuarioUseCase(IUsuarioRepositorio usuarios, ISesionRepositorio sesiones, ITransaccionIdentidad transaccion,
    ProteccionAdministradores proteccion, AuditarIdentidad auditoria, TimeProvider reloj)
{
    internal Task<ResultadoGestion> EjecutarAsync(Guid id, string motivo, ActorIdentidad actor, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(motivo) || motivo.Length > 200) return Task.FromResult(new ResultadoGestion("datos_invalidos", 400));
        return transaccion.EjecutarAsync(async () =>
        {
            var usuario = await usuarios.PorIdAsync(id, ct);
            if (usuario is null) return new ResultadoGestion("no_encontrado", 404);
            var error = await proteccion.ValidarRetiroAsync(usuario, actor.Id, ct);
            if (error is not null) return new ResultadoGestion(error, 409);
            usuario.Desactivar();
            await sesiones.RevocarUsuarioAsync(id, reloj.GetUtcNow(), ct);
            await auditoria.EscrituraAsync(AccionAuditada.CuentaDesactivada, actor, id, null, ct);
            return new ResultadoGestion();
        }, ct, protegerAdministradores: true);
    }
}
