using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Domain;
using Microsoft.Extensions.Options;

namespace KillaUru.Identidad.Application.Cuenta;

/// <summary>Recuperación e invitación revocan sesiones y desbloquean solo con un enlace válido.</summary>
internal sealed class FijarContrasenaConEnlaceUseCase(ResolverEnlace resolver,
    ISesionRepositorio sesiones, IHashContrasenas hash, ITransaccionIdentidad transaccion,
    AuditarIdentidad auditoria, IOptions<IdentidadOptions> opciones, TimeProvider reloj)
{
    private static readonly ResultadoGestion EnlaceInvalido = new("enlace_invalido", 400);

    internal Task<ResultadoGestion> EjecutarAsync(string token, string contrasena, CancellationToken ct)
    {
        if (!opciones.Value.ContrasenaValida(contrasena)) return Task.FromResult(new ResultadoGestion("datos_invalidos", 400));
        var credencial = hash.Hash(contrasena);
        return transaccion.EjecutarAsync(() => FijarAsync(token, credencial, ct), ct);
    }

    private async Task<ResultadoGestion> FijarAsync(string token, string credencial, CancellationToken ct)
    {
        var (usuario, enlace) = await resolver.CargarAsync(token, ct);
        if (enlace is null || usuario is not { Activo: true }) return EnlaceInvalido;
        var ahora = reloj.GetUtcNow();
        // Un enlace de verificación no sirve para fijar contraseña: cada enlace tiene una sola finalidad.
        var finalidadPermitida = enlace.Proposito != PropositoToken.Verificacion;
        if (!finalidadPermitida || !enlace.PuedeUsarse(ahora)) return EnlaceInvalido;
        enlace.Usar(ahora);
        usuario.FijarContrasena(credencial);
        // Quien abre la invitación demuestra que el correo es suyo.
        if (enlace.Proposito == PropositoToken.Invitacion) usuario.MarcarCorreoVerificado();
        await sesiones.RevocarUsuarioAsync(usuario.Id, ahora, ct);
        await auditoria.EscrituraAsync(AccionAuditada.ContrasenaCambiada, null, usuario.Id, null, ct);
        return new();
    }
}
