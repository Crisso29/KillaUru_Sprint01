using System.ComponentModel.DataAnnotations;
namespace KillaUru.Identidad.Adapters.Dtos;

/// <summary>Entrada documentada de AceptacionSolicitud; Application valida las reglas de negocio.</summary>
internal sealed class AceptacionSolicitud
{
    /// <summary>Documento de la solicitud.</summary>
    /// <example>PoliticaPrivacidad</example>
    [Required]
    public required string Documento { get; init; }

    /// <summary>Version de la solicitud.</summary>
    /// <example>1.2</example>
    [Required, MaxLength(100)]
    public required string Version { get; init; }
}
