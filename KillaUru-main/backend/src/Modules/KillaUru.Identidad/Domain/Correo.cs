namespace KillaUru.Identidad.Domain;

/// <summary>Correo normalizado; la creación inválida devuelve null, sin excepciones de negocio.</summary>
internal sealed record Correo(string Valor)
{
    internal static Correo? Crear(string? valor)
    {
        var normalizado = valor?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(normalizado) || normalizado.Length > 254 || normalizado.Any(char.IsWhiteSpace)) return null;
        var partes = normalizado.Split('@');
        if (partes.Length != 2 || partes[0].Length == 0 || !partes[1].Contains('.', StringComparison.Ordinal)) return null;
        if (partes[1].Split('.').Any(string.IsNullOrEmpty)) return null;
        return new Correo(normalizado);
    }
}
