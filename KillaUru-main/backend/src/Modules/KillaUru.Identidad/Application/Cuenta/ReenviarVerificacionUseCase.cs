using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Application.Cuenta;

/// <summary>Reemplaza el enlace propio sin emitir otro si el correo ya está verificado.</summary>
internal sealed class ReenviarVerificacionUseCase(IUsuarioRepositorio usuarios, EmitirEnlaceCorreo enlaces, ITransaccionIdentidad transaccion)
{
    internal Task<ResultadoGestion> EjecutarAsync(Guid id, CancellationToken ct) => transaccion.EjecutarAsync(async () =>
    {
        var usuario = await usuarios.PorIdAsync(id, ct);
        if (usuario is null || !usuario.Activo) return new ResultadoGestion("token_invalido", 401);
        if (!usuario.CorreoVerificado) await enlaces.CrearAsync(usuario, PropositoToken.Verificacion, true, ct);
        return new ResultadoGestion(Estado: 202);
    }, ct);
}
