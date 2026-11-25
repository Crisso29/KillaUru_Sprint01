using KillaUru.Auditoria.Adapters.Documentacion;
using KillaUru.Auditoria.Application;
using KillaUru.Auditoria.Application.Puertos;
using KillaUru.Auditoria.Contracts;
using KillaUru.Auditoria.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace KillaUru.Auditoria;

/// <summary>Raíz de composición del módulo Auditoria: única puerta para registrar sus servicios.</summary>
public static class AuditoriaModule
{
    /// <summary>Registra los servicios del módulo. Lee solo su propia sección de configuración.</summary>
    /// <param name="services">Contenedor de dependencias del host.</param>
    /// <param name="configuration">Configuración del host.</param>
    /// <returns>El mismo contenedor, para encadenar registros.</returns>
    public static IServiceCollection AddAuditoriaModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddDbContext<AuditoriaDbContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));
        services.AddScoped<FabricaContextoAuditoria>();
        services.AddScoped<IRegistroAuditoriaRepositorio, RegistroAuditoriaRepositorio>();
        services.AddScoped<IConsultaAuditoria, ConsultaAuditoriaRepositorio>();
        services.AddScoped<RegistrarAuditoriaUseCase>();
        services.AddScoped<RegistrarLecturaUseCase>();
        services.AddScoped<ConsultarAuditoriaUseCase>();
        services.AddScoped<IAuditoriaService, AuditoriaService>();
        services.AddOpenApi("v1", opciones => opciones
            .AddSchemaTransformer<EsquemasAuditoria>().AddOperationTransformer<OperacionAuditoria>());
        return services;
    }
}
