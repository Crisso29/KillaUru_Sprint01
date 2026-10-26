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
        var aceptoPoliticaVigente = entrada.VersionPoliticaAceptada == opciones.Value.VersionPolitica;
        var aceptoTerminosVigentes = entrada.VersionTerminosAceptada == opciones.Value.VersionTerminos;
        if (!aceptoPoliticaVigente || !aceptoTerminosVigentes)
            return Task.FromResult(new ResultadoLogin(Error: "aceptacion_requerida", Estado: 400));
        // El hash (PBKDF2, costoso) se calcula fuera de la transacción para no alargar el bloqueo.
        var credencial = hash.Hash(entrada.Contrasena);
        return transaccion.EjecutarAsync(() => RegistrarAsync(correo, credencial, ct), ct);
    }

    private async Task<ResultadoLogin> RegistrarAsync(Correo correo, string credencial, CancellationToken ct)
    {
        if (await usuarios.PorCorreoAsync(correo.Valor, ct) is not null) return new(Error: "correo_en_uso", Estado: 409);
        var ahora = reloj.GetUtcNow();
        var usuario = Usuario.RegistrarConductor(correo, credencial, ahora);
        usuarios.Agregar(usuario);
        var politica = Consentimiento.Crear(usuario.Id, DocumentoLegal.PoliticaPrivacidad, opciones.Value.VersionPolitica, ahora);
        var terminos = Consentimiento.Crear(usuario.Id, DocumentoLegal.TerminosUso, opciones.Value.VersionTerminos, ahora);
        await usuarios.AceptarAsync(politica, ct);
        await usuarios.AceptarAsync(terminos, ct);
        var sesion = emision.Crear(usuario);
        await enlaces.CrearAsync(usuario, PropositoToken.Verificacion, true, ct);
        await auditoria.EscrituraAsync(AccionAuditada.UsuarioCreado, null, usuario.Id,
            new Dictionary<string, object?> { ["rol"] = usuario.Rol.ToString() }, ct);
        return new(sesion, Estado: 201);
    }
}
