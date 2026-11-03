using KillaUru.Ingesta.Application.Procesamiento;

namespace KillaUru.Ingesta.Application.Puertos;

/// <summary>Cola en memoria acotada; solo transfiere avisos de loteId, nunca eventos completos (ADR-005 punto 3).</summary>
internal interface IColaLotes
{
    /// <summary>True si aún hay espacio para escribir un aviso sin bloquear.</summary>
    bool HayCapacidad();

    /// <summary>Intenta escribir el aviso; devuelve false si la cola está llena.</summary>
    bool TryEscribir(LoteRecibido aviso);

    /// <summary>Enumera los avisos de forma asíncrona; se detiene cuando se cancela <paramref name="ct"/>.</summary>
    IAsyncEnumerable<LoteRecibido> LeerAsync(CancellationToken ct);
}
