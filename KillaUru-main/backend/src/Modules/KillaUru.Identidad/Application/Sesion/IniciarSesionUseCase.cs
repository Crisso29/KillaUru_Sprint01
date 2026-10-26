using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Domain;
using Microsoft.Extensions.Options;

namespace KillaUru.Identidad.Application.Sesion;

/// <summary>Autentica sin revelar cuentas de conductores y registra los intentos fallidos atómicamente.</summary>
internal sealed class IniciarSesionUseCase(IUsuarioRepositorio usuarios, IHashContrasenas hash,
    IVerificadorAntiBot antiBot, ITransaccionIdentidad transaccion, EmisionSesion emision,
    AuditarIdentidad auditoria, IOptions<IdentidadOptions> opciones, TimeProvider reloj)
{
    internal async Task<ResultadoLogin> EjecutarAsync(InicioSesionEntrada entrada, CancellationToken ct)
    {
        var canalConocido = entrada.Canal is ("app" or "panel");
        // Rechazar contraseñas enormes antes de PBKDF2 evita usar el hash como amplificador de CPU.
        var contrasenaAbusiva = entrada.Contrasena.Length > opciones.Value.ContrasenaMaxima;
        if (!canalConocido || contrasenaAbusiva) return new(Error: "credenciales_invalidas", Estado: 401);
        if (entrada.Canal == "panel" && !await antiBot.VerificarAsync(entrada.TurnstileToken, ct))
            return new(Error: "antibot_fallido", Estado: 403);
        return await transaccion.EjecutarAsync(() => AutenticarAsync(entrada, ct), ct);
    }

    private async Task<ResultadoLogin> AutenticarAsync(InicioSesionEntrada entrada, CancellationToken ct)
    {
        var correo = Correo.Crear(entrada.Correo);
        var usuario = correo is null ? null : await usuarios.PorCorreoAsync(correo.Valor, ct);
        var valida = hash.Verificar(usuario?.HashContrasena, entrada.Contrasena, out var rehash);
        var ahora = reloj.GetUtcNow();
        // Correo inexistente, clave incorrecta, cuenta desactivada o bloqueada: misma respuesta (RN-19, FR-008).
        if (usuario is null || !valida || !usuario.PuedeIniciarSesion(ahora))
            return await RechazarAsync(usuario, entrada.Canal, ct);
        if (entrada.Canal == "panel" && usuario.Rol != RolUsuario.Administrador)
            return await RechazarCanalAsync(usuario, ct);
        usuario.Bloqueo.RegistrarExito();
        if (rehash) usuario.FijarContrasena(hash.Hash(entrada.Contrasena));
        if (usuario.Rol == RolUsuario.Administrador)
            await auditoria.EscrituraAsync(AccionAuditada.InicioSesionAdministrador, new(usuario.Id, usuario.Rol.ToString()), usuario.Id, null, ct);
        return new(emision.Crear(usuario));
    }

    private async Task<ResultadoLogin> RechazarAsync(Usuario? usuario, string canal, CancellationToken ct)
    {
        var ahora = reloj.GetUtcNow();
        var cuentaBloqueable = usuario is { Activo: true, HashContrasena: not null };
        var bloqueoNuevo = cuentaBloqueable && usuario!.Bloqueo.RegistrarFallo(ahora, ParametrosBloqueo());
        await auditoria.EscrituraAsync(AccionAuditada.InicioSesionFallido, null, usuario?.Id, null, ct);
        if (bloqueoNuevo) await auditoria.EscrituraAsync(AccionAuditada.CuentaBloqueada, null, usuario!.Id, null, ct);
        // Solo el panel muestra el bloqueo; la app recibe lo mismo que con credenciales inválidas (FR-011).
        var adminBloqueadoEnPanel = canal == "panel"
            && usuario is { Activo: true, Rol: RolUsuario.Administrador } && usuario.Bloqueo.EstaBloqueado(ahora);
        if (adminBloqueadoEnPanel)
            return new(Error: "cuenta_bloqueada", Estado: 423, BloqueadoHastaUtc: usuario!.Bloqueo.BloqueadoHasta);
        return new(Error: "credenciales_invalidas", Estado: 401);
    }

    private ParametrosBloqueo ParametrosBloqueo()
    {
        var parametros = opciones.Value;
        return new(parametros.IntentosMaximos, parametros.VentanaIntentosMinutos, parametros.DuracionesBloqueoMinutos);
    }

    private async Task<ResultadoLogin> RechazarCanalAsync(Usuario usuario, CancellationToken ct)
    {
        await auditoria.EscrituraAsync(AccionAuditada.InicioSesionFallido, null, usuario.Id, null, ct);
        return new(Error: "solo_administradores", Estado: 403);
    }
}
