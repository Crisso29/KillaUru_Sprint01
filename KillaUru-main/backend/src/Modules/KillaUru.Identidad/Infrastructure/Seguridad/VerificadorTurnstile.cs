using System.Text.Json;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Puertos;
using Microsoft.Extensions.Options;

namespace KillaUru.Identidad.Infrastructure.Seguridad;

/// <summary>Falla cerrada ante respuestas inválidas o indisponibilidad; nunca registra el token.</summary>
internal sealed class VerificadorTurnstile(HttpClient cliente, IOptions<IdentidadOptions> opciones) : IVerificadorAntiBot
{
    private const string UrlVerificacion = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

    /// <summary>Cloudflare documenta tokens de hasta 2048 caracteres; uno más largo no es legítimo.</summary>
    private const int LargoMaximoToken = 2048;

    public async Task<bool> VerificarAsync(string? token, CancellationToken ct)
    {
        var tokenValido = !string.IsNullOrWhiteSpace(token) && token.Length <= LargoMaximoToken;
        var secretoConfigurado = !string.IsNullOrWhiteSpace(opciones.Value.TurnstileSecreto);
        if (!tokenValido || !secretoConfigurado) return false;
        try
        {
            using var cuerpo = new FormUrlEncodedContent(new Dictionary<string, string>
                { ["secret"] = opciones.Value.TurnstileSecreto, ["response"] = token! });
            using var respuesta = await cliente.PostAsync(UrlVerificacion, cuerpo, ct);
            if (!respuesta.IsSuccessStatusCode) return false;
            using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(ct));
            return json.RootElement.TryGetProperty("success", out var exito) && exito.ValueKind == JsonValueKind.True;
        }
        catch (HttpRequestException) { return false; }
        catch (JsonException) { return false; }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { return false; }
    }
}
