using System.ComponentModel.DataAnnotations;
namespace KillaUru.Identidad.Adapters.Dtos;

/// <summary>Entrada documentada de InicioSesionSolicitud; Application valida las reglas de negocio.</summary>
internal sealed class InicioSesionSolicitud
{
    /// <summary>Correo de la solicitud.</summary>
    /// <example>conductor@ejemplo.pe</example>
    [Required, MaxLength(254)]
    public required string Correo { get; init; }

    /// <summary>Contrasena de la solicitud.</summary>
    /// <example>una-clave-larga</example>
    [Required, MaxLength(128)]
    public required string Contrasena { get; init; }

    /// <summary>Canal de la solicitud.</summary>
    /// <example>app</example>
    [Required, RegularExpression("^(app|panel)$")]
    public required string Canal { get; init; }

    /// <summary>TurnstileToken de la solicitud.</summary>
    /// <example>desafio-turnstile</example>
    [MaxLength(2048)]
    public string? TurnstileToken { get; init; }
}
