using KillaUru.Anomalias.Contracts;

namespace KillaUru.Ingesta.Application.Recepcion;

/// <summary>
/// Aplica la regla «un lote = un seudónimo» (FR-024, D2):
/// la cabecera gana; sin cabecera, el primer evento con seudónimo válido fija el del lote.
/// </summary>
internal static class SeudonimoDelLote
{
    /// <summary>Determina el seudónimo del lote a partir de la cabecera y los eventos ya leídos.</summary>
    internal static Guid? Resolver(Guid? cabecera, IEnumerable<EventoReportado> eventos)
    {
        if (cabecera.HasValue) return cabecera;
        return eventos.FirstOrDefault(e => e.Seudonimo.HasValue)?.Seudonimo;
    }

    /// <summary>
    /// Devuelve true si el evento trae un seudónimo distinto al del lote.
    /// Solo se llama cuando el evento tiene eventoId válido (R-010).
    /// </summary>
    internal static bool EsInconsistente(Guid seudonimoLote, EventoReportado evento) =>
        evento.Seudonimo.HasValue && evento.Seudonimo.Value != seudonimoLote;
}
