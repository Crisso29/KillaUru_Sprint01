using System.ComponentModel.DataAnnotations;
namespace KillaUru.Identidad.Adapters.Dtos;

/// <summary>Entrada documentada de CambiarRolSolicitud; Application valida las reglas de negocio.</summary>
internal sealed class CambiarRolSolicitud
{
    /// <summary>Rol de la solicitud.</summary>
    /// <example>ConductorRegistrado</example>
    [Required]
    public required string Rol { get; init; }
}
