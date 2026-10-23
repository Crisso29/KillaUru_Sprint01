using System.Text.Json;
namespace KillaUru.Auditoria.Domain;

/// <summary>Inspecciona objetos y listas completos; anidar un dato no evita la regla de privacidad.</summary>
internal static class ClavesProhibidas
{
    private static readonly HashSet<string> Nombres = new(StringComparer.OrdinalIgnoreCase)
        { "correo", "email", "contrasena", "password", "seudonimo" };

    internal static bool Contiene(JsonElement valor) => valor.ValueKind switch
    {
        JsonValueKind.Object => valor.EnumerateObject().Any(p => Nombres.Contains(p.Name) || Contiene(p.Value)),
        JsonValueKind.Array => valor.EnumerateArray().Any(Contiene),
        _ => false,
    };
}
