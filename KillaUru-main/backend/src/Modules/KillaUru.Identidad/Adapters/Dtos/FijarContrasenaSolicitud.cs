using System.ComponentModel.DataAnnotations;
namespace KillaUru.Identidad.Adapters.Dtos;

/// <summary>Entrada documentada de FijarContrasenaSolicitud; Application valida las reglas de negocio.</summary>
internal sealed class FijarContrasenaSolicitud
{
    /// <summary>Token de la solicitud.</summary>
    /// <example>token-base64url</example>
    [Required, MaxLength(256)]
    public required string Token { get; init; }

    /// <summary>NuevaContrasena de la solicitud.</summary>
    /// <example>nueva-clave-larga</example>
    [Required, MaxLength(128)]
    public required string NuevaContrasena { get; init; }
}
