using Microsoft.Extensions.Caching.Hybrid;
using KillaUru.Mapa.Application.Puertos;
using KillaUru.Mapa.Domain;
using KillaUru.Mapa.Application;
namespace KillaUru.Mapa.Infrastructure;
/// <summary>
/// Decorador de <see cref="IFuenteCeldas"/> con HybridCache; clave «celda:{id}:{alcance}»,
/// etiqueta «celda:{id}» para invalidar ambos alcances con un solo evict (FR-006, FR-007).
/// </summary>
internal sealed class FuenteCeldasCacheada(
    FuenteCeldasAnomalias origen,
    HybridCache cache,
    IMetricasMapa metricas,
    Microsoft.Extensions.Options.IOptions<MapaOptions> opciones) : IFuenteCeldas
{
    private readonly TimeSpan _vigencia = TimeSpan.FromMinutes(opciones.Value.VigenciaCacheMinutos);
    public async Task<byte[]> ObtenerAsync(long celda, Alcance alcance, CancellationToken ct)
    {
        var clave    = $"celda:{celda}:{alcance}";
        var etiqueta = $"celda:{celda}";
        bool fallo = false;
        var bytes = await cache.GetOrCreateAsync(clave,
            async token =>
            {
                fallo = true;
                metricas.RegistrarCache("fallo");
                return await origen.ObtenerAsync(celda, alcance, token);
            },
            new HybridCacheEntryOptions { Expiration = _vigencia },
            tags: [etiqueta],
            cancellationToken: ct);
        if (!fallo)
        {
            metricas.RegistrarCache("acierto");
        }
        return bytes;
    }
    /// <summary>Invalida las entradas de caché para ambos alcances de la celda.</summary>
    internal async Task InvalidarCeldaAsync(long celda, CancellationToken ct) =>
        await cache.RemoveByTagAsync($"celda:{celda}", ct);
}
