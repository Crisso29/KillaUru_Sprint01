using System.Text.Json;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Puertos;
using Microsoft.Extensions.Options;

namespace KillaUru.Identidad.Infrastructure.Seguridad;

/// <summary>Falla cerrada ante respuestas inválidas o indisponibilidad; nunca registra el token.</summary>
internal sealed class VerificadorTurnstile(HttpClient cliente, IOptions<IdentidadOptions> opciones) : IVerificadorAntiBot
{
    public async Task<bool> VerificarAsync(string? token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 2048 || string.IsNullOrWhiteSpace(opciones.Value.TurnstileSecreto)) return false;
        try
        {
            using var cuerpo = new FormUrlEncodedContent(new Dictionary<string, string>
                { ["secret"] = opciones.Value.TurnstileSecreto, ["response"] = token });
            using var respuesta = await cliente.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", cuerpo, ct);
            if (!respuesta.IsSuccessStatusCode) return false;
            using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(ct));
            return json.RootElement.TryGetProperty("success", out var exito) && exito.ValueKind == JsonValueKind.True;
        }
        catch (HttpRequestException) { return false; }
        catch (JsonException) { return false; }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { return false; }
    }
}
