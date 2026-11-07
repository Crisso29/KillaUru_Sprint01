using KillaUru.Anomalias.Contracts;
using KillaUru.Mapa.Application;
using KillaUru.Mapa.Application.Puertos;
using KillaUru.Mapa.Domain;
using KillaUru.Mapa.Infrastructure;
using KillaUru.Shared.Eventos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KillaUru.Mapa;

/// <summary>Raíz de composición del módulo Mapa: única puerta para registrar sus servicios.</summary>
public static class MapaModule
{
    /// <summary>Registra los servicios del módulo. Lee solo su propia sección de configuración.</summary>
    public static IServiceCollection AddMapaModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        // Opciones
        services.AddOptions<MapaOptions>()
                .Bind(configuration.GetSection("Mapa"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
        // HybridCache (nivel 1 = memoria; sin nivel 2)
        services.AddHybridCache();
        RegistrarDominio(services);
        RegistrarInfraestructura(services);
        // Use cases (transient — sin estado)
        services.AddTransient<ConsultarPorCeldasUseCase>();
        services.AddTransient<ConsultarFiltradoUseCase>();
        services.AddTransient<ConsultarCambiosUseCase>();
        services.AddTransient<ConsultarMapaUseCase>();
        return services;
    }

    private static void RegistrarDominio(IServiceCollection services)
    {
        // Dominio y aplicación (singleton — sin estado mutable excepto CorredorMapa)
        services.AddSingleton(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<MapaOptions>>().Value;
            var poligono = new PoligonoGeo(opts.PoligonoDemo);
            return new ZonaDemo(poligono);
        });
        services.AddSingleton<IVersionesCelda>(sp =>
            new VersionesCelda(sp.GetRequiredService<TimeProvider>()));
        services.AddSingleton<MetricasMapa>();
        services.AddSingleton<IMetricasMapa>(sp => sp.GetRequiredService<MetricasMapa>());
        // Los lectores del contrato de Anomalías comparten el scope de su DbContext.
        services.AddScoped<CorredorMapa>();
        services.AddSingleton<CalculadorETag>();

    }

    private static void RegistrarInfraestructura(IServiceCollection services)
    {
        // Infraestructura
        services.AddScoped<FuenteCeldasAnomalias>();
        services.AddScoped<FuenteCeldasCacheada>();
        services.AddScoped<IFuenteCeldas>(sp => sp.GetRequiredService<FuenteCeldasCacheada>());
        services.AddSingleton<VersionesCelda>(sp =>
            (VersionesCelda)sp.GetRequiredService<IVersionesCelda>());
        // Invalidación de caché (Observer)
        services.AddScoped<InvalidarCeldasMapa>();
        services.AddScoped<IManejador<AnomaliaCambiada>>(sp =>
            sp.GetRequiredService<InvalidarCeldasMapa>());

    }
}
