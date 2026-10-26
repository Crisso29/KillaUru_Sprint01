using KillaUru.Identidad.Application.Resultados;
using Microsoft.AspNetCore.Http;

namespace KillaUru.Identidad.Adapters;

/// <summary>
/// Cookies del panel: HttpOnly (el JavaScript no las lee), Secure y SameSite=Strict (sin CSRF).
/// La de renovación solo viaja a su ruta, así no se expone en cada petición (FR-020).
/// </summary>
internal static class CookiesSesion
{
    /// <summary>Debe coincidir con <c>AutenticacionExtensions.CookieAcceso</c> del host, que la valida.</summary>
    internal const string Acceso = "ku_acceso";
    internal const string Renovacion = "ku_renovacion";
    private const string RutaRenovacion = "/api/v1/auth/renovar";

    internal static void Escribir(HttpResponse respuesta, SesionEmitida sesion)
    {
        respuesta.Cookies.Append(Acceso, sesion.TokenAcceso, Opciones("/", sesion.ExpiraAccesoUtc));
        respuesta.Cookies.Append(Renovacion, sesion.TokenRenovacion, Opciones(RutaRenovacion, sesion.SesionVenceUtc));
    }

    /// <summary>Para borrar una cookie hay que repetir la misma ruta con la que se escribió.</summary>
    internal static void Borrar(HttpResponse respuesta)
    {
        respuesta.Cookies.Delete(Acceso, Opciones("/", null));
        respuesta.Cookies.Delete(Renovacion, Opciones(RutaRenovacion, null));
    }

    private static CookieOptions Opciones(string ruta, DateTimeOffset? vence) => new()
    {
        HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Path = ruta, Expires = vence, IsEssential = true,
    };
}
