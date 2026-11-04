using KillaUru.Anomalias.Contracts;
using KillaUru.Mapa.Application.Puertos;
using KillaUru.Shared.Eventos;

namespace KillaUru.Mapa.Infrastructure;

/// <summary>
/// Reacciona a <see cref="AnomaliaCambiada"/>: incrementa la versión y evicta la caché (FR-007).
/// </summary>
internal sealed class InvalidarCeldasMapa(
    FuenteCeldasCacheada cache,
    IVersionesCelda versiones) : IManejador<AnomaliaCambiada>
{
    public async Task ManejarAsync(AnomaliaCambiada evento, CancellationToken ct)
    {
        versiones.Incrementar(evento.Celda.Id);
        await cache.InvalidarCeldaAsync(evento.Celda.Id, ct);
    }
}
