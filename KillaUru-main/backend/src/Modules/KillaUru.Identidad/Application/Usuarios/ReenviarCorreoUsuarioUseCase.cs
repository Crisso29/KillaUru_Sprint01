using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application.Cuenta;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Domain;
namespace KillaUru.Identidad.Application.Usuarios;

/// <summary>El reenvío conserva el propósito pendiente e invalida el enlace anterior.</summary>
internal sealed class ReenviarCorreoUsuarioUseCase(IUsuarioRepositorio usuarios, ITransaccionIdentidad transaccion,
    EmitirEnlaceCorreo enlaces, AuditarIdentidad auditoria)
{
    internal Task<ResultadoGestion> EjecutarAsync(Guid id, ActorIdentidad actor, CancellationToken ct) =>
        transaccion.EjecutarAsync(async () =>
        {
            var usuario = await usuarios.PorIdAsync(id, ct);
            if (usuario is null) return new ResultadoGestion("no_encontrado", 404);
            var proposito = usuario.HashContrasena is null ? PropositoToken.Invitacion : PropositoToken.Verificacion;
            await enlaces.CrearAsync(usuario, proposito, true, ct);
            await auditoria.EscrituraAsync(AccionAuditada.RecuperacionSolicitada, actor, id,
                new Dictionary<string, object?> { ["operacion"] = "Reenvio", ["proposito"] = proposito.ToString() }, ct);
            return new ResultadoGestion(Estado: 202);
        }, ct);
}
