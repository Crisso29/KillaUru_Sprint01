using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Mapa;

/// <summary>Raíz de composición del módulo Mapa: única puerta para registrar sus servicios.</summary>
public static class MapaModule
{
    /// <summary>Registra los servicios del módulo. Lee solo su propia sección de configuración.</summary>
    /// <param name="services">Contenedor de dependencias del host.</param>
    /// <param name="configuration">Configuración del host.</param>
    /// <returns>El mismo contenedor, para encadenar registros.</returns>
    public static IServiceCollection AddMapaModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return services;
    }
}
