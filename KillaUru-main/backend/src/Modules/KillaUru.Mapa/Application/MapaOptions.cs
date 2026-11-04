using System.ComponentModel.DataAnnotations;

namespace KillaUru.Mapa.Application;

/// <summary>Parámetros de configuración del módulo Mapa (sección «Mapa»; spec 006 §MapaOptions).</summary>
internal sealed class MapaOptions
{
    /// <summary>Vértices del polígono demo en formato GeoJSON [lon, lat]; mínimo 4 puntos (R-010).</summary>
    [Required, MinLength(4)]
    public double[][] PoligonoDemo { get; init; } = [];

    /// <summary>Vigencia de cada entrada del HybridCache (ADR-006).</summary>
    [Range(1, 60)]
    public int VigenciaCacheMinutos { get; init; } = 10;

    /// <summary>max-age de las respuestas públicas anónimas (ADR-006).</summary>
    [Range(1, 3600)]
    public int MaxAgeBordeSegundos { get; init; } = 60;

    /// <summary>Límite de memoria del HybridCache (ADR-006).</summary>
    [Range(1, 512)]
    public int TamanoCacheMb { get; init; } = 64;
}
