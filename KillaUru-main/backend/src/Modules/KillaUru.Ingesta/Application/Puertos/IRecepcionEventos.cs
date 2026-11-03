using KillaUru.Ingesta.Application.Recepcion;

namespace KillaUru.Ingesta.Application.Puertos;

/// <summary>Puerto de escritura: guarda los eventos aceptados con idempotencia (ADR-005).</summary>
internal interface IRecepcionEventos
{
    /// <summary>Inserta los eventos del lote en un solo INSERT ON CONFLICT DO NOTHING y devuelve los ids realmente insertados.</summary>
    Task<IReadOnlySet<Guid>> InsertarAsync(LoteAceptado lote, CancellationToken ct);
}
