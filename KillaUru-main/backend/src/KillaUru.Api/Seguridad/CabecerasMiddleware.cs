namespace KillaUru.Api.Seguridad;

/// <summary>
/// Cabeceras comunes (FR-013, FR-014): HSTS y nosniff siempre; <c>Cache-Control: private, no-store</c>
/// por defecto. Solo un endpoint anónimo que lo declare a propósito puede ser público (ADR-006, feature 006):
/// así nunca se cachea en el borde una respuesta con datos de una cuenta.
/// </summary>
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
            // Solo un endpoint anónimo que se declara «public» a propósito se puede cachear; todo lo demás
            // (incluidos los errores, a los que ASP.NET les pone «no-cache, no-store») queda privado.
            if (!cabeceras.CacheControl.ToString().Contains("public", StringComparison.OrdinalIgnoreCase))
            {
                cabeceras.CacheControl = NoCachear;
            }

            return Task.CompletedTask;
        });
        return siguiente(contexto);
    }
}
