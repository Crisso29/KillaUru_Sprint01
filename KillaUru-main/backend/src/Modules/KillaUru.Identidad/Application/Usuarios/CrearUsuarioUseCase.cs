using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application.Cuenta;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Domain;
namespace KillaUru.Identidad.Application.Usuarios;

/// <summary>Invitación sin contraseña: el administrador nunca conoce la credencial de otro usuario.</summary>
internal sealed class CrearUsuarioUseCase(IUsuarioRepositorio usuarios, ITransaccionIdentidad transaccion,
    EmitirEnlaceCorreo enlaces, AuditarIdentidad auditoria, TimeProvider reloj)
{
    internal Task<ResultadoUsuario> EjecutarAsync(string direccion, string rol, ActorIdentidad actor, CancellationToken ct)
    {
        var correo = Correo.Crear(direccion);
        if (correo is null || rol is not ("ConductorRegistrado" or "Administrador"))
            return Task.FromResult(new ResultadoUsuario(Error: "datos_invalidos", Estado: 400));
        return transaccion.EjecutarAsync(async () =>
        {
            if (await usuarios.PorCorreoAsync(correo.Valor, ct) is not null) return new ResultadoUsuario(Error: "correo_en_uso", Estado: 409);
            var usuario = Usuario.Invitar(correo, Enum.Parse<RolUsuario>(rol), reloj.GetUtcNow());
            usuarios.Agregar(usuario);
            await enlaces.CrearAsync(usuario, PropositoToken.Invitacion, true, ct);
            await auditoria.EscrituraAsync(AccionAuditada.UsuarioCreado, actor, usuario.Id, new Dictionary<string, object?> { ["rol"] = rol }, ct);
            return new ResultadoUsuario(ProyectarUsuario.Vista(usuario));
        }, ct, protegerAdministradores: true);
    }
}
