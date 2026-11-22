using System.Text.Json;

namespace KillaUru.Integracion.Tests.Ingesta;

/// <summary>Lee un error de la API: exige `application/problem+json` (RFC 9457) y devuelve su `codigo`.</summary>
internal static class ProblemaLeido
{
    internal static async Task<string?> CodigoAsync(HttpResponseMessage respuesta)
    {
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        using var cuerpo = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        return cuerpo.RootElement.GetProperty("codigo").GetString();
    }
}
