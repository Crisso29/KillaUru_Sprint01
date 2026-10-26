using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Domain;
using Microsoft.Extensions.Options;

namespace KillaUru.Identidad.Application.Sesion;

/// <summary>Fábrica de familia y credenciales compartida por registro, login y renovación.</summary>
internal sealed class EmisionSesion(ISesionRepositorio sesiones, IGeneradorSecretos secretos,
    IEmisorJwt jwt, IOptions<IdentidadOptions> opciones, TimeProvider reloj)
{
    internal SesionEmitida Crear(Usuario usuario)
    {
        var ahora = reloj.GetUtcNow();
        var limiteAbsoluto = LimiteAbsoluto(usuario, ahora);
        return Agregar(usuario, Guid.CreateVersion7(ahora), secretos.Aleatorio32(), limiteAbsoluto, ahora);
    }

    /// <summary>Solo el administrador tiene un tope de 8 h que no se extiende al renovar (FR-015).</summary>
    private DateTimeOffset? LimiteAbsoluto(Usuario usuario, DateTimeOffset ahora) =>
        usuario.Rol == RolUsuario.Administrador ? ahora.AddHours(opciones.Value.SesionAdministradorHoras) : null;

    internal SesionEmitida Agregar(Usuario usuario, Guid familia, string token, DateTimeOffset? limite, DateTimeOffset ahora)
    {
        var vence = PoliticaSesion.Vencimiento(ahora, limite, opciones.Value.SesionConductorDias, opciones.Value.SesionAdministradorHoras);
        var sesion = SesionRenovacion.Crear(usuario.Id, familia, secretos.Hash(token), ahora, vence, limite);
        sesiones.Agregar(sesion);
        return Par(usuario, sesion, token, ahora);
    }

    internal SesionEmitida Par(Usuario usuario, SesionRenovacion sesion, string token, DateTimeOffset ahora)
    {
        var vence = ahora.AddMinutes(opciones.Value.TokenAccesoMinutos);
        // El JWT de acceso tampoco puede sobrevivir al límite absoluto del administrador.
        if (sesion.LimiteAbsolutoUtc is { } limite && limite < vence) vence = limite;
        var jwtAcceso = jwt.Emitir(usuario, sesion.FamiliaId, ahora, vence);
        return new(jwtAcceso, vence, token, sesion.ExpiraUtc, usuario.Rol.ToString());
    }
}
