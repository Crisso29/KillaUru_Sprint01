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
        DateTimeOffset? limite = usuario.Rol == RolUsuario.Administrador ? ahora.AddHours(opciones.Value.SesionAdministradorHoras) : null;
        return Agregar(usuario, Guid.CreateVersion7(ahora), secretos.Aleatorio32(), limite, ahora);
    }

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
        if (sesion.LimiteAbsolutoUtc is { } limite && limite < vence) vence = limite;
        return new(jwt.Emitir(usuario, sesion.FamiliaId, ahora, vence), vence, token, sesion.ExpiraUtc, usuario.Rol.ToString());
    }
}
