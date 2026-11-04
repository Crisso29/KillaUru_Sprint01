using KillaUru.Dashboard.Application;
using KillaUru.Dashboard.Application.Puertos;
using KillaUru.Dashboard.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Dashboard;

/// <summary>Raíz de composición del módulo Dashboard: única puerta para registrar sus servicios.</summary>
public static class DashboardModule
{
    /// <summary>Registra los servicios del módulo. Lee solo su propia sección de configuración.</summary>
    /// <param name="services">Contenedor de dependencias del host.</param>
    /// <param name="configuration">Configuración del host.</param>
    /// <returns>El mismo contenedor, para encadenar registros.</returns>
    public static IServiceCollection AddDashboardModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<DashboardOptions>(configuration.GetSection(DashboardOptions.Seccion));

        services.AddDbContext<DashboardDbContext>(o =>
            o.UseNpgsql(configuration.GetConnectionString("KillaUru")));

        services.AddSingleton<CalculadoUtcHolder>();
        services.AddScoped<IIndicadoresRepositorio, IndicadoresRepositorio>();
        services.AddScoped<CalcularIndicadoresUseCase>();
        services.AddScoped<ConsultarResumenUseCase>();
        services.AddHostedService<TareaIndicadoresDiarios>();

        return services;
    }
}
