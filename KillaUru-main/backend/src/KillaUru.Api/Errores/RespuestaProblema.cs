namespace KillaUru.Api.Errores;

/// <summary>
/// Escribe un error como ProblemDetails (RFC 9457) pasando por <see cref="IProblemDetailsService"/>,
/// así todo error lleva el mismo <c>codigo</c>, <c>type</c> y <c>correlacionId</c> (constitución IX).
/// </summary>
internal static class RespuestaProblema
{
    internal static ValueTask EscribirAsync(HttpContext contexto, int estado, string codigo, string titulo)
    {
        contexto.Response.StatusCode = estado;
        var servicio = contexto.RequestServices.GetRequiredService<IProblemDetailsService>();
        return servicio.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = contexto,
            ProblemDetails = { Status = estado, Title = titulo, Extensions = { ["codigo"] = codigo } },
        });
    }
}
