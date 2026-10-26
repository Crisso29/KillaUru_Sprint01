using System.ComponentModel.DataAnnotations;
namespace KillaUru.Identidad.Adapters.Dtos;

/// <summary>Entrada documentada de RegistroSolicitud; Application valida las reglas de negocio.</summary>
internal sealed class RegistroSolicitud
{
    /// <summary>Correo de la solicitud.</summary>
    /// <example>conductor@ejemplo.pe</example>
    [Required, EmailAddress, MaxLength(254)]
    public required string Correo { get; init; }

    /// <summary>Contrasena de la solicitud.</summary>
    /// <example>una-clave-larga</example>
    [Required, MaxLength(128)]
    public required string Contrasena { get; init; }

    /// <summary>VersionPoliticaAceptada de la solicitud.</summary>
    /// <example>1.2</example>
    
    public string? VersionPoliticaAceptada { get; init; }

    /// <summary>VersionTerminosAceptada de la solicitud.</summary>
    /// <example>1.2</example>
    
    public string? VersionTerminosAceptada { get; init; }
}
