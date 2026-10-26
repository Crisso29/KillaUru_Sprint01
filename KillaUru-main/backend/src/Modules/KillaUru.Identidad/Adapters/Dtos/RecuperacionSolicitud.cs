using System.ComponentModel.DataAnnotations;
namespace KillaUru.Identidad.Adapters.Dtos;

/// <summary>Entrada documentada de RecuperacionSolicitud; Application valida las reglas de negocio.</summary>
internal sealed class RecuperacionSolicitud
{
    /// <summary>Correo de la solicitud.</summary>
    /// <example>conductor@ejemplo.pe</example>
    [Required, MaxLength(254)]
    public required string Correo { get; init; }
}
