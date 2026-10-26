namespace KillaUru.Identidad.Tests.Infrastructure;

/// <summary>Transporte sustituido: prueba adaptadores HTTP sin enviar mensajes ni secretos fuera del proceso.</summary>
internal sealed class RespuestaHttpPrueba(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(responder(request));
}
