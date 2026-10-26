using System.ComponentModel.DataAnnotations;
namespace KillaUru.Identidad.Adapters.Dtos;

/// <summary>Entrada documentada de RevisionSolicitud; Application valida las reglas de negocio.</summary>
internal sealed class RevisionSolicitud
{
    /// <summary>Confirmados de la solicitud.</summary>
    /// <example>[]</example>
    [Required, MaxLength(32767)]
    public required Guid[] Confirmados { get; init; }

    /// <summary>Retirados de la solicitud.</summary>
    /// <example>[]</example>
    [Required, MaxLength(32767)]
    public required Guid[] Retirados { get; init; }
}
