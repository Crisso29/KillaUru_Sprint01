using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Puertos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KillaUru.Identidad.Infrastructure.Correo;

/// <summary>Resend con reintentos e idempotencia; un fallo de correo no cambia el resultado ya confirmado.</summary>
internal sealed class EnviadorCorreoHttp(HttpClient cliente, IOptions<IdentidadOptions> opciones,
    TimeProvider reloj, ILogger<EnviadorCorreoHttp> logger) : IEnviadorCorreo
{
    private static readonly Action<ILogger, Exception?> Fallo = LoggerMessage.Define(LogLevel.Error,
        new EventId(3002, "CorreoNoEnviado"), "No se pudo enviar un correo de identidad; disponible reenvío.");
    public Task VerificacionAsync(string correo, string enlace, CancellationToken ct) => EnviarAsync(correo, enlace, "verificar", ct);
    public Task RecuperacionAsync(string correo, string enlace, CancellationToken ct) => EnviarAsync(correo, enlace, "recuperar", ct);
    public Task InvitacionAsync(string correo, string enlace, CancellationToken ct) => EnviarAsync(correo, enlace, "activar", ct);

    private async Task EnviarAsync(string correo, string enlace, string finalidad, CancellationToken ct)
    {
        var idempotencia = Guid.NewGuid().ToString();
        for (var intento = 0; intento < 4; intento++)
        {
            var resultado = await IntentarAsync(correo, enlace, finalidad, idempotencia, ct);
            if (resultado == ResultadoEnvioCorreo.Enviado) return;
            if (resultado == ResultadoEnvioCorreo.Definitivo) break;   // un 4xx no cambia reintentando: solo demora al usuario
            if (intento < 3) await Task.Delay(TimeSpan.FromMilliseconds(250 * (intento + 1)), reloj, ct);
        }
        // No se agrega la excepción del proveedor: podría contener dirección, cuerpo o cabeceras.
        Fallo(logger, null);
    }

    private async Task<ResultadoEnvioCorreo> IntentarAsync(string correo, string enlace, string finalidad, string clave, CancellationToken ct)
    {
        try
        {
            using var peticion = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
            peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", opciones.Value.CorreoApiKey);
            peticion.Headers.Add("Idempotency-Key", clave);
            peticion.Content = JsonContent.Create(new { from = opciones.Value.CorreoRemitente, to = new[] { correo },
                subject = "KillaUru: " + finalidad + " cuenta", text = PlantillasCorreo.Texto(finalidad, enlace) });
            using var respuesta = await cliente.SendAsync(peticion, ct);
            return Clasificar(respuesta.StatusCode);
        }
        catch (HttpRequestException) { return ResultadoEnvioCorreo.Pasajero; }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { return ResultadoEnvioCorreo.Pasajero; }
    }

    /// <summary>R-007 reintenta cuando falla el proveedor (5xx, 408, 429, red), no cuando rechaza la petición (4xx).</summary>
    private static ResultadoEnvioCorreo Clasificar(HttpStatusCode estado) => (int)estado switch
    {
        >= 200 and < 300 => ResultadoEnvioCorreo.Enviado,
        408 or 429 or >= 500 => ResultadoEnvioCorreo.Pasajero,
        _ => ResultadoEnvioCorreo.Definitivo,
    };

}
