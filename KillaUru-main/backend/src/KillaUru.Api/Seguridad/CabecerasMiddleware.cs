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
        // OnStarting registra el callback pero no lo ejecuta ahora: se dispara justo antes de enviar
        // la primera línea de la respuesta, cuando la autenticación ya resolvió la identidad (ADR-006).
        contexto.Response.OnStarting(() =>
        {
            AplicarCabeceras(contexto);
            return Task.CompletedTask;
        });
        return siguiente(contexto);
    }

    /// <summary>
    /// Aplica HSTS, nosniff y la directiva de caché correcta.
    /// Los usuarios autenticados nunca deben quedar en cachés intermedios (ADR-006, FR-013).
    /// Solo las respuestas públicas explícitas conservan Cache-Control: public.
    /// </summary>
    private static void AplicarCabeceras(HttpContext contexto)
    {
        var cabeceras = contexto.Response.Headers;
        cabeceras.StrictTransportSecurity = Hsts;
        cabeceras.XContentTypeOptions     = "nosniff";

        var usuarioAutenticado   = contexto.User.Identity?.IsAuthenticated == true;
        var respuestaPublicaExplicita = cabeceras.CacheControl.ToString()
            .Split(',').Any(d => d.Trim().Equals("public", StringComparison.OrdinalIgnoreCase));

        // Sobrescribir con no-store a menos que sea una respuesta pública para un anónimo
        if (usuarioAutenticado || !respuestaPublicaExplicita)
        {
            cabeceras.CacheControl = NoCachear;
        }
    }
}
