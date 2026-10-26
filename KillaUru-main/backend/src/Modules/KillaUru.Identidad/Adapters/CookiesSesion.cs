using KillaUru.Identidad.Application.Resultados;
using Microsoft.AspNetCore.Http;
namespace KillaUru.Identidad.Adapters;

/// <summary>Cookies restringidas al servidor y al sitio; refresh solo circula por su endpoint.</summary>
internal static class CookiesSesion
{
    internal static void Escribir(HttpResponse respuesta, SesionEmitida sesion)
    {
        respuesta.Cookies.Append("ku_acceso", sesion.TokenAcceso, Opciones("/", sesion.ExpiraAccesoUtc));
        respuesta.Cookies.Append("ku_renovacion", sesion.TokenRenovacion, Opciones("/api/v1/auth/renovar", sesion.SesionVenceUtc));
    }

    internal static void Borrar(HttpResponse respuesta)
    {
        respuesta.Cookies.Delete("ku_acceso", Opciones("/", null));
        respuesta.Cookies.Delete("ku_renovacion", Opciones("/api/v1/auth/renovar", null));
    }

    private static CookieOptions Opciones(string ruta, DateTimeOffset? vence) => new()
    {
        HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Path = ruta, Expires = vence, IsEssential = true,
    };
}
