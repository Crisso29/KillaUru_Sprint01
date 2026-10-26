using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Domain;
using Microsoft.Extensions.Options;

namespace KillaUru.Identidad.Application.Cuenta;

/// <summary>Emite el hash dentro de la transacción y agenda el envío solo después del commit.</summary>
internal sealed class EmitirEnlaceCorreo(ITokenCorreoRepositorio tokens, IGeneradorSecretos secretos,
    IEnviadorCorreo correo, ITransaccionIdentidad transaccion, IOptions<IdentidadOptions> opciones, TimeProvider reloj)
{
    internal async Task<string> CrearAsync(Usuario usuario, PropositoToken proposito, bool enviar, CancellationToken ct)
    {
        var ahora = reloj.GetUtcNow();
        var secreto = secretos.Aleatorio32();
        await tokens.InvalidarAsync(usuario.Id, proposito, ahora, ct);
        tokens.Agregar(TokenCorreo.Crear(secretos.Hash(secreto), usuario.Id, proposito, ahora, Vigencia(proposito)));
        // El token va en el fragmento (#): el navegador no lo envía al servidor ni queda en logs (FR-024).
        var enlace = $"{opciones.Value.UrlBaseEnlaces.TrimEnd('/')}/{Ruta(proposito)}#token={secreto}";
        if (enviar) transaccion.AlConfirmar(() => EnviarAsync(usuario.Correo.Valor, enlace, proposito));
        return enlace;
    }

    private static string Ruta(PropositoToken proposito) => proposito switch
    {
        PropositoToken.Verificacion => "verificar",
        PropositoToken.Recuperacion => "restablecer",
        _ => "invitacion",
    };

    private TimeSpan Vigencia(PropositoToken proposito) => proposito switch
    {
        PropositoToken.Verificacion => TimeSpan.FromHours(opciones.Value.VerificacionCorreoHoras),
        PropositoToken.Recuperacion => TimeSpan.FromMinutes(opciones.Value.RecuperacionMinutos),
        _ => TimeSpan.FromHours(opciones.Value.InvitacionHoras),
    };

    private Task EnviarAsync(string destino, string enlace, PropositoToken proposito) => proposito switch
    {
        PropositoToken.Verificacion => correo.VerificacionAsync(destino, enlace, CancellationToken.None),
        PropositoToken.Recuperacion => correo.RecuperacionAsync(destino, enlace, CancellationToken.None),
        _ => correo.InvitacionAsync(destino, enlace, CancellationToken.None),
    };
}
