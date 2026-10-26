using System.ComponentModel.DataAnnotations;
namespace KillaUru.Identidad.Adapters.Dtos;

/// <summary>Entrada documentada de RenovarSolicitud; Application valida las reglas de negocio.</summary>
internal sealed class RenovarSolicitud
{
    /// <summary>TokenRenovacion de la solicitud.</summary>
    /// <example>token-base64url</example>
    [MaxLength(256)]
    public string? TokenRenovacion { get; init; }
}
