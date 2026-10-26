using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application.Cuenta;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Domain;
namespace KillaUru.Identidad.Application.Usuarios;

/// <summary>Semilla de uso único: el enlace se devuelve al comando y no se envía a ningún logger.</summary>
internal sealed class CrearAdministradorInicialUseCase(IUsuarioRepositorio usuarios, ITransaccionIdentidad transaccion,
    EmitirEnlaceCorreo enlaces, AuditarIdentidad auditoria, TimeProvider reloj)
{
    internal Task<ResultadoEnlace> EjecutarAsync(string direccion, CancellationToken ct)
    {
        var correo = Correo.Crear(direccion);
        if (correo is null) return Task.FromResult(new ResultadoEnlace(Error: "datos_invalidos"));
        return transaccion.EjecutarAsync(async () =>
        {
            var existentes = await usuarios.ListarAsync(new(Rol: "Administrador", Tamano: 1), ct);
            if (existentes.Total != 0) return new ResultadoEnlace(Error: "administrador_existente");
            if (await usuarios.PorCorreoAsync(correo.Valor, ct) is not null) return new ResultadoEnlace(Error: "correo_en_uso");
            var usuario = Usuario.Invitar(correo, RolUsuario.Administrador, reloj.GetUtcNow());
            usuarios.Agregar(usuario);
            var enlace = await enlaces.CrearAsync(usuario, PropositoToken.Invitacion, false, ct);
            await auditoria.EscrituraAsync(AccionAuditada.UsuarioCreado, null, usuario.Id,
                new Dictionary<string, object?> { ["rol"] = "Administrador" }, ct);
            return new ResultadoEnlace(enlace);
        }, ct, protegerAdministradores: true);
    }
}
