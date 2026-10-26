using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Application.Cuenta;

/// <summary>Respuesta idéntica incluso al superar cuotas: no revela registro ni verificación de una cuenta.</summary>
internal sealed class SolicitarRecuperacionUseCase(IUsuarioRepositorio usuarios, ILimitadorRecuperacion limite,
    EmitirEnlaceCorreo enlaces, ITransaccionIdentidad transaccion, AuditarIdentidad auditoria)
{
    internal Task<ResultadoGestion> EjecutarAsync(string direccion, string ip, CancellationToken ct)
    {
        var correo = Correo.Crear(direccion);
        if (correo is null || !limite.Admitir(correo.Valor, ip)) return Task.FromResult(new ResultadoGestion(Estado: 202));
        return transaccion.EjecutarAsync(async () =>
        {
            var usuario = await usuarios.PorCorreoAsync(correo.Valor, ct);
            if (usuario is { CorreoVerificado: true, Activo: true })
                await enlaces.CrearAsync(usuario, PropositoToken.Recuperacion, true, ct);
            await auditoria.EscrituraAsync(AccionAuditada.RecuperacionSolicitada, null, usuario?.Id, null, ct);
            return new ResultadoGestion(Estado: 202);
        }, ct);
    }
}
