using System.Net;
using System.Net.Http.Json;

namespace KillaUru.Integracion.Tests.Ingesta;

/// <summary>Envía un lote que debe responder 200 y devuelve su cuerpo leído como lo hace la app.</summary>
internal static class EnvioLote
{
    internal static async Task<RespuestaLoteLeida> EnviarAsync(HttpClient cliente, HttpRequestMessage peticion)
    {
        using var respuesta = await cliente.SendAsync(peticion, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        return (await respuesta.Content.ReadFromJsonAsync<RespuestaLoteLeida>(TestContext.Current.CancellationToken))!;
    }
}
