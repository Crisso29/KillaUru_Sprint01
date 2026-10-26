using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Domain;
using Microsoft.Extensions.Logging;

namespace KillaUru.Identidad.Application.Titular;

/// <summary>Borrado en cascada con auditoría y log posterior al commit, ambos sin correo.</summary>
internal sealed class EliminarMiCuentaUseCase(IUsuarioRepositorio usuarios, IHashContrasenas hash,
    ITransaccionIdentidad transaccion, AuditarIdentidad auditoria, ILogger<EliminarMiCuentaUseCase> logger)
{
    private static readonly Action<ILogger, Guid, Exception?> RegistrarEliminacion = LoggerMessage.Define<Guid>(
        LogLevel.Information, new EventId(3001, "CuentaEliminada"), "CuentaEliminada {UsuarioId}");
    internal Task<ResultadoGestion> EjecutarAsync(Guid id, string contrasena, CancellationToken ct) =>
        transaccion.EjecutarAsync(async () =>
        {
            var usuario = await usuarios.PorIdAsync(id, ct);
            if (usuario is null || !usuario.Activo) return new ResultadoGestion("token_invalido", 401);
            if (usuario.Rol != RolUsuario.ConductorRegistrado || !hash.Verificar(usuario.HashContrasena, contrasena, out _))
                return new ResultadoGestion("confirmacion_invalida", 403);
            usuarios.Eliminar(usuario);
            await auditoria.EscrituraAsync(AccionAuditada.CuentaEliminada, new(id, usuario.Rol.ToString()), id, null, ct);
            transaccion.AlConfirmar(() => { RegistrarEliminacion(logger, id, null); return Task.CompletedTask; });
            return new ResultadoGestion();
        }, ct);
}
