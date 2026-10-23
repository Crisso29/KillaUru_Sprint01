namespace KillaUru.Api.Seguridad;

/// <summary>
/// Cabeceras comunes (FR-013, FR-014): HSTS y nosniff siempre; <c>Cache-Control: private, no-store</c>
/// por defecto. Solo conserva una directiva public explícita para peticiones anónimas.
/// </summary>
/// <remarks>
/// Se decide al enviar la respuesta, cuando la autenticación ya estableció la identidad (ADR-006).
/// </remarks>
/// <param name="siguiente">Resto del pipeline.</param>
internal sealed class CabecerasMiddleware(RequestDelegate siguiente)
{
    private const string Hsts = "max-age=31536000; includeSubDomains";
    private const string NoCachear = "private, no-store";

    public Task InvokeAsync(HttpContext contexto)
    {
        contexto.Response.OnStarting(() =>
        {
            var cabeceras = contexto.Response.Headers;
            cabeceras.StrictTransportSecurity = Hsts;
            cabeceras.XContentTypeOptions = "nosniff";
            // OnStarting aplica las cabeceras justo antes de enviarlas, también en respuestas de error.
            if (contexto.User.Identity?.IsAuthenticated == true || !cabeceras.CacheControl.ToString()
                .Split(',').Any(directiva => directiva.Trim().Equals("public", StringComparison.OrdinalIgnoreCase)))
            {
                cabeceras.CacheControl = NoCachear;
            }

            return Task.CompletedTask;
        });
        return siguiente(contexto);
    }
}
