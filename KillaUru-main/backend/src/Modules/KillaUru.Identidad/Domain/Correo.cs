namespace KillaUru.Identidad.Domain;

/// <summary>Correo normalizado; la creación inválida devuelve null, sin excepciones de negocio.</summary>
internal sealed record Correo(string Valor)
{
    /// <summary>Máximo de RFC 5321 para una dirección completa.</summary>
    private const int LargoMaximo = 254;

    internal static Correo? Crear(string? valor)
    {
        var normalizado = valor?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(normalizado)) return null;
        var largoValido = normalizado.Length <= LargoMaximo;
        var sinEspacios = !normalizado.Any(char.IsWhiteSpace);
        return largoValido && sinEspacios && TieneUsuarioYDominio(normalizado) ? new Correo(normalizado) : null;
    }

    /// <summary>Una sola arroba, usuario no vacío y un dominio con al menos un punto sin etiquetas vacías.</summary>
    private static bool TieneUsuarioYDominio(string correo)
    {
        var partes = correo.Split('@');
        if (partes.Length != 2) return false;
        var (usuario, dominio) = (partes[0], partes[1]);
        var etiquetasDominio = dominio.Split('.');
        return usuario.Length > 0 && etiquetasDominio.Length > 1 && !etiquetasDominio.Any(string.IsNullOrEmpty);
    }
}
