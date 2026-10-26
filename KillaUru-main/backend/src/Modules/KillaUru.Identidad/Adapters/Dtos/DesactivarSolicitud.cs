using System.ComponentModel.DataAnnotations;
namespace KillaUru.Identidad.Adapters.Dtos;

/// <summary>Entrada documentada de DesactivarSolicitud; Application valida las reglas de negocio.</summary>
internal sealed class DesactivarSolicitud
{
    /// <summary>Motivo de la solicitud.</summary>
    /// <example>Fin de funciones</example>
    [Required, MaxLength(200)]
    public required string Motivo { get; init; }
}
