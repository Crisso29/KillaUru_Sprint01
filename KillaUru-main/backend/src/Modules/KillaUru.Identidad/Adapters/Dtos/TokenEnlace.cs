using System.ComponentModel.DataAnnotations;
namespace KillaUru.Identidad.Adapters.Dtos;

/// <summary>Entrada documentada de TokenEnlace; Application valida las reglas de negocio.</summary>
internal sealed class TokenEnlace
{
    /// <summary>Token de la solicitud.</summary>
    /// <example>token-base64url</example>
    [Required, MaxLength(256)]
    public required string Token { get; init; }
}
