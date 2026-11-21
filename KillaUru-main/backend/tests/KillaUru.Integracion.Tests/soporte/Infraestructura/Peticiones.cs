using System.Net.Http.Headers;

namespace KillaUru.Integracion.Tests.Infraestructura;

/// <summary>Atajos para armar peticiones de prueba con token, cookie o seudónimo.</summary>
internal static class Peticiones
{
    internal static HttpRequestMessage Get(string ruta, string? token = null, string? seudonimo = null) =>
        Crear(HttpMethod.Get, ruta, token, seudonimo);

    internal static HttpRequestMessage Post(string ruta, string? token = null, string? seudonimo = null) =>
        Crear(HttpMethod.Post, ruta, token, seudonimo);

    internal static HttpRequestMessage ConCookie(string ruta, string token)
    {
        var peticion = new HttpRequestMessage(HttpMethod.Get, ruta);
        peticion.Headers.Add("Cookie", $"ku_acceso={token}");
        return peticion;
    }

    /// <summary>Envía <paramref name="veces"/> peticiones iguales y devuelve el código de la última.</summary>
    internal static async Task<System.Net.HttpStatusCode> RepetirAsync(HttpClient cliente, int veces, Func<HttpRequestMessage> crear)
    {
        var ultimo = System.Net.HttpStatusCode.OK;
        for (var i = 0; i < veces; i++)
        {
            using var respuesta = await cliente.SendAsync(crear(), TestContext.Current.CancellationToken);
            ultimo = respuesta.StatusCode;
        }

        return ultimo;
    }

    private static HttpRequestMessage Crear(HttpMethod metodo, string ruta, string? token, string? seudonimo)
    {
        var peticion = new HttpRequestMessage(metodo, ruta);
        if (token is not null)
        {
            peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (seudonimo is not null)
        {
            peticion.Headers.Add("X-KillaUru-Seudonimo", seudonimo);
        }

        return peticion;
    }
}
