using KillaUru.Ingesta.Application.Procesamiento;
using KillaUru.Ingesta.Domain;

namespace KillaUru.Ingesta.Application.Puertos;

/// <summary>Puerto de los eventos pendientes de procesar (trabajador, conteo RN-15a).</summary>
internal interface IPendientesRepositorio
{
    /// <summary>Devuelve los eventos Recibido del lote en orden de detectado_utc, evento_id.</summary>
    Task<IReadOnlyList<EventoPendiente>> RecibidosDelLoteAsync(Guid? loteId, CancellationToken ct);

    /// <summary>Cuenta los eventos del seudónimo con detectado_utc en [inicioHora, inicioHora+1h) (RN-15a).</summary>
    Task<int> ContarEnLaHoraAsync(Guid seudonimo, DateTimeOffset inicioHora, CancellationToken ct);

    /// <summary>
    /// Actualiza todos los eventos del lote en un único commit (ADR-005 punto 4, optimización DT-01).
    /// Los eventos con fallo de procesamiento no deben incluirse: quedan Recibido para el barredor (R-015).
    /// No hace nada si la lista está vacía.
    /// </summary>
    Task MarcarLoteAsync(IReadOnlyList<MarcaEvento> marcas, CancellationToken ct);
}
