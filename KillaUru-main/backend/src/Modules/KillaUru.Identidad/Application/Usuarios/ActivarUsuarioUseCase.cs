using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
namespace KillaUru.Identidad.Application.Usuarios;

/// <summary>Activar no revive sesiones revocadas; será necesario iniciar sesión de nuevo.</summary>
internal sealed class ActivarUsuarioUseCase(IUsuarioRepositorio usuarios, ITransaccionIdentidad transaccion, AuditarIdentidad auditoria)
{
    internal Task<ResultadoGestion> EjecutarAsync(Guid id, ActorIdentidad actor, CancellationToken ct) =>
        transaccion.EjecutarAsync(async () =>
        {
            var usuario = await usuarios.PorIdAsync(id, ct);
            if (usuario is null) return new ResultadoGestion("no_encontrado", 404);
            usuario.Activar();
            await auditoria.EscrituraAsync(AccionAuditada.CuentaActivada, actor, id,
                new Dictionary<string, object?> { ["rol"] = usuario.Rol.ToString() }, ct);
            return new ResultadoGestion();
        }, ct, protegerAdministradores: true);
}
