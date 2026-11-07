using KillaUru.Anomalias.Contracts;

namespace KillaUru.Ingesta.Application.Recepcion;

/// <summary>Lote guardado: cada evento en exactamente una de las tres listas.</summary>
internal sealed record ResultadoLoteAceptado(
    Guid LoteId,
    IReadOnlyList<Guid> Aceptados,
    IReadOnlyList<Guid> Duplicados,
    IReadOnlyList<Rechazo> Rechazados) : ResultadoLote;
