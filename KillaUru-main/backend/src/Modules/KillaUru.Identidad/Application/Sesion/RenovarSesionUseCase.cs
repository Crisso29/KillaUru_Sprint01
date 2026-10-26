using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Domain;
using Microsoft.Extensions.Options;

namespace KillaUru.Identidad.Application.Sesion;

/// <summary>Rotación serializada: dos solicitudes simultáneas obtienen el mismo sucesor.</summary>
internal sealed class RenovarSesionUseCase(ISesionRepositorio sesiones, IUsuarioRepositorio usuarios,
    IGeneradorSecretos secretos, ITransaccionIdentidad transaccion, EmisionSesion emision,
    IOptions<IdentidadOptions> opciones, TimeProvider reloj)
{
    internal Task<ResultadoRenovacion> EjecutarAsync(string? token, CancellationToken ct) =>
        string.IsNullOrEmpty(token) || token.Length > 256 ? Task.FromResult(new ResultadoRenovacion(Error: "sesion_invalida")) :
            transaccion.EjecutarAsync(() => RenovarAsync(token, ct), ct);

    private async Task<ResultadoRenovacion> RenovarAsync(string token, CancellationToken ct)
    {
        var hash = secretos.Hash(token);
        var id = await sesiones.UsuarioPorHashAsync(hash, ct);
        var usuario = id is null ? null : await usuarios.PorIdAsync(id.Value, ct);
        if (usuario is null) return new(Error: "sesion_invalida");
        // Leer el token después del bloqueo de la cuenta evita reutilizar una generación obsoleta.
        var sesion = await sesiones.PorHashAsync(hash, ct);
        if (sesion is null) return new(Error: "sesion_invalida");
        var error = sesion.Validar(reloj.GetUtcNow(), usuario.Activo, opciones.Value.GraciaRotacionSegundos);
        // Una cuenta desactivada se informa aun si la operación administrativa ya revocó la familia.
        if (!usuario.Activo) error = "cuenta_desactivada";
        if (error is not null)
        {
            await sesiones.RevocarFamiliaAsync(sesion.FamiliaId, reloj.GetUtcNow(), ct);
            return new(Error: error);
        }
        return await RotarAsync(usuario, sesion, token, ct);
    }

    private async Task<ResultadoRenovacion> RotarAsync(Usuario usuario, SesionRenovacion sesion, string token, CancellationToken ct)
    {
        var nuevo = secretos.Derivar(token);
        var ahora = reloj.GetUtcNow();
        if (sesion.UsadoUtc is not null)
        {
            var sucesor = await sesiones.PorHashAsync(secretos.Hash(nuevo), ct);
            if (sucesor is null || sucesor.Validar(ahora, usuario.Activo, opciones.Value.GraciaRotacionSegundos) is not null)
                return new(Error: "sesion_invalida");
            return new(emision.Par(usuario, sucesor, nuevo, ahora));
        }
        sesion.Usar(ahora);
        return new(emision.Agregar(usuario, sesion.FamiliaId, nuevo, sesion.LimiteAbsolutoUtc, ahora));
    }
}
