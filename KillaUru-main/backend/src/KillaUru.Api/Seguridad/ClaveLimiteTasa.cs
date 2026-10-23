using KillaUru.Shared.Domain;

namespace KillaUru.Api.Seguridad;

/// <summary>
/// Calcula a quién se le cuenta cada petición (RNF-11): la cuenta si hay sesión; si no, el seudónimo
/// de la cabecera más la IP real (tolera redes CGNAT, donde muchos celulares comparten IP).
/// </summary>
internal static class ClaveLimiteTasa
{
    internal const string CabeceraSeudonimo = "X-KillaUru-Seudonimo";

    /// <summary>Id de la cuenta, o <c>null</c> si la petición es anónima.</summary>
    internal static string? Cuenta(HttpContext contexto) =>
        contexto.User.Identity?.IsAuthenticated == true
            ? contexto.User.FindFirst(AutenticacionExtensions.ClaimSujeto)?.Value
            : null;

    internal static bool EsAdministrador(HttpContext contexto) =>
        contexto.User.FindFirst(AutenticacionExtensions.ClaimRol)?.Value == Roles.Administrador;

    /// <summary>IP real (ya resuelta por <see cref="IpRealExtensions"/>).</summary>
    internal static string Ip(HttpContext contexto) => contexto.Connection.RemoteIpAddress?.ToString() ?? "sin-ip";

    /// <summary>Clave del anónimo: seudónimo válido de la cabecera + IP.</summary>
    internal static string SeudonimoMasIp(HttpContext contexto)
    {
        var seudonimo = SeudonimoInstalacion.Crear(contexto.Request.Headers[CabeceraSeudonimo].ToString());
        return $"{seudonimo?.Valor.ToString() ?? "sin-seudonimo"}|{Ip(contexto)}";
    }
}
