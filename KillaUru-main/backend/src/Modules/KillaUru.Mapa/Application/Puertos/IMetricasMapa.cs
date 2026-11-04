namespace KillaUru.Mapa.Application.Puertos;

/// <summary>Puerto de métricas del módulo Mapa (R-013; killauru.mapa.cache).</summary>
internal interface IMetricasMapa
{
    /// <summary>
    /// Registra el resultado de caché de una petición.
    /// <paramref name="resultado"/> ∈ {"acierto", "fallo", "no_modificado"}.
    /// </summary>
    void RegistrarCache(string resultado);
}
