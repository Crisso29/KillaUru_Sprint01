using System.Collections.Concurrent;
using KillaUru.Mapa.Application.Puertos;

namespace KillaUru.Mapa.Infrastructure;

/// <summary>Contador de versiones por celda en memoria; hilo seguro con <see cref="ConcurrentDictionary{TKey,TValue}"/>.</summary>
internal sealed class VersionesCelda(TimeProvider reloj) : IVersionesCelda
{
    private readonly ConcurrentDictionary<long, long> _versiones = new();

    public DateTimeOffset InstanteArranque { get; } = reloj.GetUtcNow();

    public long ObtenerVersion(long celda) =>
        _versiones.TryGetValue(celda, out var v) ? v : 0L;

    public void Incrementar(long celda) =>
        _versiones.AddOrUpdate(celda, 1L, (_, anterior) => anterior + 1L);
}
