using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Application.Sesion;
using KillaUru.Identidad.Domain;
using Microsoft.Extensions.Options;

namespace KillaUru.Identidad.Application.Cuenta;

/// <summary>Registro atómico con consentimiento, sesión no verificada y verificación enviada tras confirmar.</summary>
internal sealed class RegistrarConductorUseCase(IUsuarioRepositorio usuarios, IHashContrasenas hash,
    ITransaccionIdentidad transaccion, EmisionSesion emision, EmitirEnlaceCorreo enlaces,
    AuditarIdentidad auditoria, IOptions<IdentidadOptions> opciones, TimeProvider reloj)
{
    internal Task<ResultadoLogin> EjecutarAsync(RegistroEntrada entrada, CancellationToken ct)
    {
        var correo = Correo.Crear(entrada.Correo);
        if (correo is null || !opciones.Value.ContrasenaValida(entrada.Contrasena))
            return Task.FromResult(new ResultadoLogin(Error: "datos_invalidos", Estado: 400));
        if (entrada.VersionPoliticaAceptada != opciones.Value.VersionPolitica || entrada.VersionTerminosAceptada != opciones.Value.VersionTerminos)
            return Task.FromResult(new ResultadoLogin(Error: "aceptacion_requerida", Estado: 400));
        var credencial = hash.Hash(entrada.Contrasena);
        return transaccion.EjecutarAsync(() => RegistrarAsync(correo, credencial, ct), ct);
    }

    private async Task<ResultadoLogin> RegistrarAsync(Correo correo, string credencial, CancellationToken ct)
    {
        if (await usuarios.PorCorreoAsync(correo.Valor, ct) is not null) return new(Error: "correo_en_uso", Estado: 409);
        var usuario = Usuario.RegistrarConductor(correo, credencial, reloj.GetUtcNow());
        usuarios.Agregar(usuario);
        await usuarios.AceptarAsync(Consentimiento.Crear(usuario.Id, DocumentoLegal.PoliticaPrivacidad, opciones.Value.VersionPolitica, reloj.GetUtcNow()), ct);
        await usuarios.AceptarAsync(Consentimiento.Crear(usuario.Id, DocumentoLegal.TerminosUso, opciones.Value.VersionTerminos, reloj.GetUtcNow()), ct);
        var sesion = emision.Crear(usuario);
        await enlaces.CrearAsync(usuario, PropositoToken.Verificacion, true, ct);
        await auditoria.EscrituraAsync(AccionAuditada.UsuarioCreado, null, usuario.Id,
            new Dictionary<string, object?> { ["rol"] = usuario.Rol.ToString() }, ct);
        return new(sesion, Estado: 201);
    }
}
