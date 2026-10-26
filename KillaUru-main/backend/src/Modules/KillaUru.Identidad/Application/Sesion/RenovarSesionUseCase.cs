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
    /// <summary>Un token legítimo mide 43 caracteres (32 bytes en Base64Url); el tope corta cuerpos abusivos.</summary>
    private const int LargoMaximoToken = 256;

    private static readonly ResultadoRenovacion SesionInvalida = new(Error: ErroresSesion.SesionInvalida);

    internal Task<ResultadoRenovacion> EjecutarAsync(string? token, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(token) || token.Length > LargoMaximoToken) return Task.FromResult(SesionInvalida);
        return transaccion.EjecutarAsync(() => RenovarAsync(token, ct), ct);
    }

    private async Task<ResultadoRenovacion> RenovarAsync(string token, CancellationToken ct)
    {
        var hash = secretos.Hash(token);
        var usuarioId = await sesiones.UsuarioPorHashAsync(hash, ct);
        var usuario = usuarioId is null ? null : await usuarios.PorIdAsync(usuarioId.Value, ct);
        if (usuario is null) return SesionInvalida;
        // Leer el token después del bloqueo de la cuenta evita reutilizar una generación obsoleta.
        var sesion = await sesiones.PorHashAsync(hash, ct);
        if (sesion is null) return SesionInvalida;
        var ahora = reloj.GetUtcNow();
        var error = sesion.Validar(ahora, usuario.Activo, opciones.Value.GraciaRotacionSegundos);
        // Una cuenta desactivada se informa aun si la operación administrativa ya revocó la familia.
        if (!usuario.Activo) error = ErroresSesion.CuentaDesactivada;
        if (error is null) return await RotarAsync(usuario, sesion, token, ct);
        await sesiones.RevocarFamiliaAsync(sesion.FamiliaId, ahora, ct);
        return new(Error: error);
    }

    /// <summary>
    /// Si la sesión ya se usó, es un reintento dentro de la gracia: se devuelve el sucesor ya emitido
    /// en vez de crear otro, para que ambos clientes queden con el mismo par (FR-016).
    /// </summary>
    private async Task<ResultadoRenovacion> RotarAsync(Usuario usuario, SesionRenovacion sesion, string token, CancellationToken ct)
    {
        var tokenSucesor = secretos.Derivar(token);
        var ahora = reloj.GetUtcNow();
        if (sesion.UsadoUtc is null)
        {
            sesion.Usar(ahora);
            return new(emision.Agregar(usuario, sesion.FamiliaId, tokenSucesor, sesion.LimiteAbsolutoUtc, ahora));
        }
        var sucesor = await sesiones.PorHashAsync(secretos.Hash(tokenSucesor), ct);
        var sucesorValido = sucesor is not null && sucesor.Validar(ahora, usuario.Activo, opciones.Value.GraciaRotacionSegundos) is null;
        return sucesorValido ? new(emision.Par(usuario, sucesor!, tokenSucesor, ahora)) : SesionInvalida;
    }
}
