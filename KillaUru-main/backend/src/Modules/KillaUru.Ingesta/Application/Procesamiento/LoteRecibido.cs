namespace KillaUru.Ingesta.Application.Procesamiento;

/// <summary>
/// Aviso en la cola: solo el loteId, nunca los eventos (ADR-005 punto 3).
/// LoteId null = filas Recibido sin lote_id que el barredor recuperó (R-011).
/// </summary>
internal sealed record LoteRecibido(Guid? LoteId);
