using System.ComponentModel.DataAnnotations;
namespace KillaUru.Identidad.Adapters.Dtos;

/// <summary>Entrada documentada de CrearUsuarioSolicitud; Application valida las reglas de negocio.</summary>
internal sealed class CrearUsuarioSolicitud
{
    /// <summary>Correo de la solicitud.</summary>
    /// <example>administrador@ejemplo.pe</example>
    [Required, EmailAddress, MaxLength(254)]
    public required string Correo { get; init; }

    /// <summary>Rol de la solicitud.</summary>
    /// <example>Administrador</example>
    [Required]
    public required string Rol { get; init; }
}
