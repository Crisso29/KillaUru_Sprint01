using System.Diagnostics.Metrics;
using KillaUru.Mapa.Application.Puertos;

namespace KillaUru.Mapa.Infrastructure;

/// <summary>Métricas del módulo Mapa: contador killauru.mapa.cache (R-013; baja cardinalidad).</summary>
internal sealed class MetricasMapa : IMetricasMapa, IDisposable
{
    private readonly Meter _meter = new("KillaUru.Mapa");
    private readonly Counter<long> _cacheCounter;

    internal MetricasMapa()
    {
        _cacheCounter = _meter.CreateCounter<long>("killauru.mapa.cache",
            description: "Peticiones al mapa resueltas por caché (acierto/fallo/no_modificado).");
    }

    public void RegistrarCache(string resultado) =>
        _cacheCounter.Add(1, new KeyValuePair<string, object?>("resultado", resultado));

    public void Dispose() => _meter.Dispose();
}
