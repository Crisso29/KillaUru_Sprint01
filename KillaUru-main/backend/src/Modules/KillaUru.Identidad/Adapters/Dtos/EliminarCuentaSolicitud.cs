using System.ComponentModel.DataAnnotations;
namespace KillaUru.Identidad.Adapters.Dtos;

/// <summary>Entrada documentada de EliminarCuentaSolicitud; Application valida las reglas de negocio.</summary>
internal sealed class EliminarCuentaSolicitud
{
    /// <summary>Contrasena de la solicitud.</summary>
    /// <example>una-clave-larga</example>
    [Required, MaxLength(128)]
    public required string Contrasena { get; init; }
}
