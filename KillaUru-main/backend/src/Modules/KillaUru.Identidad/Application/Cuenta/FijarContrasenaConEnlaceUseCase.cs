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
    internal Task<ResultadoGestion> EjecutarAsync(string token, string contrasena, CancellationToken ct)
    {
        if (!opciones.Value.ContrasenaValida(contrasena)) return Task.FromResult(new ResultadoGestion("datos_invalidos", 400));
        var credencial = hash.Hash(contrasena);
        return transaccion.EjecutarAsync(() => FijarAsync(token, credencial, ct), ct);
    }

    private async Task<ResultadoGestion> FijarAsync(string token, string credencial, CancellationToken ct)
    {
        var (usuario, enlace) = await resolver.CargarAsync(token, ct);
        if (enlace is null || enlace.Proposito == PropositoToken.Verificacion || !enlace.PuedeUsarse(reloj.GetUtcNow()))
            return new("enlace_invalido", 400);
        if (usuario is null || !usuario.Activo) return new("enlace_invalido", 400);
        enlace.Usar(reloj.GetUtcNow());
        usuario.FijarContrasena(credencial);
        if (enlace.Proposito == PropositoToken.Invitacion) usuario.MarcarCorreoVerificado();
        await sesiones.RevocarUsuarioAsync(usuario.Id, reloj.GetUtcNow(), ct);
        await auditoria.EscrituraAsync(AccionAuditada.ContrasenaCambiada, null, usuario.Id, null, ct);
        return new();
    }
}
