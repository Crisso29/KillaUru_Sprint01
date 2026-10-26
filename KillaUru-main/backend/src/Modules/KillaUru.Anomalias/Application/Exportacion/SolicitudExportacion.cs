using System.ComponentModel.DataAnnotations;
namespace KillaUru.Anomalias.Application.Exportacion;
/// <summary>Filtros sobre última detección; vacío en estados significa todos.</summary>
internal sealed record SolicitudExportacion(string? Formato, DateTimeOffset? DesdeUtc = null, DateTimeOffset? HastaUtc = null,
    string? Tipo = null, string[]? Estados = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Formato is not ("CSV" or "GeoJSON")) yield return new("Formato no permitido.", [nameof(Formato)]);
        if (DesdeUtc > HastaUtc) yield return new("Intervalo de fechas inválido.", [nameof(HastaUtc)]);
        if (Tipo is not (null or "Bache" or "Hundimiento")) yield return new("Tipo no permitido.", [nameof(Tipo)]);
        if (Estados?.Any(e => e is not ("Activa" or "PendienteConfirmacion" or "Expirada" or "Reparada" or "Descartada")) == true)
            yield return new("Estado no permitido.", [nameof(Estados)]);
    }
}
