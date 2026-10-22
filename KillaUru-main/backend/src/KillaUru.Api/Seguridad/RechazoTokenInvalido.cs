using KillaUru.Api.Errores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Net.Http.Headers;

namespace KillaUru.Api.Seguridad;

/// <summary>
/// FR-008: quien envía un token inválido, vencido o con rol desconocido recibe 401 aunque el endpoint
/// sea público. Sin esto, ASP.NET lo trataría como anónimo en silencio y la app nunca sabría que debe
/// renovar el token (contrato del lote: ante un 401, renovar y reenviar).
/// </summary>
/// <param name="siguiente">Resto del pipeline.</param>
internal sealed class RechazoTokenInvalido(RequestDelegate siguiente)
{
    public async Task InvokeAsync(HttpContext contexto)
    {
        // ASP.NET solo guarda el resultado cuando la autenticación sale bien; por eso, si la petición
        // trae credenciales, se pregunta explícitamente (el manejador reutiliza el resultado de la petición).
        if (TraeCredenciales(contexto.Request) && (await contexto.AuthenticateAsync().ConfigureAwait(false)).Failure is not null)
        {
            await RespuestaProblema.EscribirAsync(
                contexto, StatusCodes.Status401Unauthorized, "token_invalido", "El token de acceso no es válido o venció.").ConfigureAwait(false);
            return;
        }

        await siguiente(contexto).ConfigureAwait(false);
    }

    private static bool TraeCredenciales(HttpRequest peticion) =>
        peticion.Headers.ContainsKey(HeaderNames.Authorization) || peticion.Cookies.ContainsKey(AutenticacionExtensions.CookieAcceso);
}
