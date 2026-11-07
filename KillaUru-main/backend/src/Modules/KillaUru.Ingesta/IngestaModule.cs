using KillaUru.Ingesta.Application;
using KillaUru.Ingesta.Application.Procesamiento;
using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Ingesta.Application.Recepcion;
using KillaUru.Ingesta.Application.Tareas;
using KillaUru.Ingesta.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Ingesta;

/// <summary>Raíz de composición del módulo Ingesta: única puerta para registrar sus servicios.</summary>
public static class IngestaModule
{
    /// <summary>Registra todos los servicios del módulo. Lee solo su propia sección de configuración.</summary>
    public static IServiceCollection AddIngestaModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddOptions<IngestaOptions>()
            .Bind(configuration.GetSection("Ingesta"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddDbContext<IngestaDbContext>(o =>
            o.UseNpgsql(configuration.GetConnectionString("KillaUru")));
        RegistrarPersistencia(services);
        RegistrarProcesamiento(services);
        return services;
    }

    private static void RegistrarPersistencia(IServiceCollection services)
    {
        // Cola (singleton: compartida entre trabajadores y el controlador)
        services.AddSingleton<ColaLotesChannel>();
        services.AddSingleton<IColaLotes>(sp => sp.GetRequiredService<ColaLotesChannel>());
        // Repositorios e infraestructura (scoped por petición/scope)
        services.AddScoped<IRecepcionEventos, RecepcionEventosPostgres>();
        services.AddScoped<IPendientesRepositorio, PendientesRepositorio>();
        services.AddScoped<IBarridoPendientes, BarridoPendientesPostgres>();
        services.AddScoped<IPurgasRecepcion, PurgasRecepcionPostgres>();

    }

    private static void RegistrarProcesamiento(IServiceCollection services)
    {
        // Casos de uso
        services.AddScoped<RecibirLoteEventosUseCase>();
        services.AddScoped<ArmadorContexto>();
        services.AddScoped<ProcesarLoteUseCase>();
        services.AddScoped<BarrerPendientesUseCase>();
        services.AddScoped<PurgarSeudonimosRecepcionUseCase>();
        services.AddScoped<PurgarRecepcionUseCase>();
        // Workers y tareas
        services.AddHostedService<ProcesadorEventosBackgroundService>();
        services.AddHostedService<BarredorBackgroundService>();
        services.AddHostedService<Infrastructure.Tareas.TareaPurgaSeudonimosRecepcion>();
        services.AddHostedService<Infrastructure.Tareas.TareaPurgaRecepcion>();
        // Health check
        services.AddSingleton<ChequeoColaIngesta>();
        services.AddHealthChecks()
            .AddCheck<ChequeoColaIngesta>("ingesta-cola", tags: ["ready"]);

    }
}
