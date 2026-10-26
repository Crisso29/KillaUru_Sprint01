using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Usuarios;
using KillaUru.Identidad.Domain;
namespace KillaUru.Identidad.Application.Privilegios;

/// <summary>Valida todo el conjunto antes de modificar cuentas; retiro y constancia son atómicos.</summary>
internal sealed class RegistrarRevisionUseCase(IUsuarioRepositorio usuarios, IRevisionRepositorio revisiones, ISesionRepositorio sesiones,
    ITransaccionIdentidad transaccion, AuditarIdentidad auditoria, TimeProvider reloj)
{
    internal Task<ResultadoRevision> EjecutarAsync(Guid[] confirmados, Guid[] retirados, ActorIdentidad actor, CancellationToken ct) =>
        transaccion.EjecutarAsync(async () =>
        {
            var administradores = await usuarios.AdministradoresAsync(ct);
            var error = Validar(confirmados, retirados, administradores, actor);
            if (error is not null) return error;
            await RetirarAsync(administradores.Where(x => retirados.Contains(x.Id)), actor, ct);
            var ahora = reloj.GetUtcNow();
            revisiones.Agregar(RevisionPrivilegios.Crear(actor.Id, (short)confirmados.Length, (short)retirados.Length, ahora));
            await auditoria.EscrituraAsync(AccionAuditada.RevisionPrivilegios, actor, null,
                new Dictionary<string, object?> { ["confirmados"] = confirmados, ["retirados"] = retirados }, ct);
            var activosRestantes = administradores.Where(x => x.Activo).Select(ProyectarUsuario.Vista).ToArray();
            return new ResultadoRevision(new(ahora, RevisionPrivilegios.Vence(ahora, ahora), activosRestantes));
        }, ct, protegerAdministradores: true);

    /// <summary>Cada administrador activo debe aparecer exactamente una vez, sin retirar al revisor ni a todos.</summary>
    private static ResultadoRevision? Validar(Guid[] confirmados, Guid[] retirados,
        IReadOnlyList<Usuario> administradores, ActorIdentidad actor)
    {
        var revisados = confirmados.Concat(retirados).ToArray();
        var cabeEnLaConstancia = revisados.Length <= short.MaxValue;
        var sinRepetidos = revisados.Distinct().Count() == revisados.Length;
        var cubreATodos = revisados.ToHashSet().SetEquals(administradores.Select(x => x.Id));
        if (!(cabeEnLaConstancia && sinRepetidos && cubreATodos)) return new(Error: "revision_incompleta", Estado: 400);
        if (retirados.Contains(actor.Id)) return new(Error: "accion_sobre_si_mismo", Estado: 409);
        if (confirmados.Length == 0) return new(Error: "ultimo_administrador", Estado: 409);
        return null;
    }

    private async Task RetirarAsync(IEnumerable<Usuario> usuariosRetirados, ActorIdentidad actor, CancellationToken ct)
    {
        foreach (var usuario in usuariosRetirados)
        {
            // Dentro de la transacción PorIdAsync hace SELECT ... FOR UPDATE: bloquea la fila
            // para que nadie la modifique mientras se retira.
            await usuarios.PorIdAsync(usuario.Id, ct);
            usuario.Desactivar();
            await sesiones.RevocarUsuarioAsync(usuario.Id, reloj.GetUtcNow(), ct);
            await auditoria.EscrituraAsync(AccionAuditada.CuentaDesactivada, actor, usuario.Id, null, ct);
        }
    }
}
