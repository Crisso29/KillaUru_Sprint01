using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KillaUru.Anomalias.Application.Procesamiento;
using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Domain;
using KillaUru.Anomalias.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KillaUru.Anomalias;

/// <summary>Raíz de composición del módulo Anomalias: única puerta para registrar sus servicios.</summary>
public static class AnomaliasModule
{
    /// <summary>Registra los servicios del módulo. Lee solo su propia sección de configuración.</summary>
    /// <param name="services">Contenedor de dependencias del host.</param>
    /// <param name="configuration">Configuración del host.</param>
    /// <returns>El mismo contenedor, para encadenar registros.</returns>
    public static IServiceCollection AddAnomaliasModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddOptions<AnomaliasOptions>().Bind(configuration.GetSection("Anomalias")).ValidateOnStart();
        services.AddSingleton<IValidateOptions<AnomaliasOptions>, ValidadorAnomaliasOptions>();
        services.AddDbContext<AnomaliasDbContext>(o => o.UseNpgsql(configuration.GetConnectionString("KillaUru"), n => n.UseNetTopologySuite()));
        services.AddOpenApi("v1", o => o.AddSchemaTransformer<Adapters.Documentacion.EsquemasAnomalias>().AddOperationTransformer<Adapters.Documentacion.OperacionAnomalias>());
        services.AddSingleton<EstadoCorredor>();
        services.AddScoped<IAnomaliaRepository, AnomaliaRepositoryPostgis>();
        services.AddScoped<IReporteRepository, ReporteRepositoryPostgis>();
        services.AddScoped<ITransaccionAnomalias, TransaccionAnomalias>();
        services.AddScoped<EvaluarIntrinsecaUseCase>();
        services.AddScoped<ProcesarReporteUseCase>();
        services.AddScoped<IAnomaliasService, AnomaliasService>();
        RegistrarLecturasYTareas(services);
        RegistrarAdministracion(services);
        services.AddHostedService<CargadorCorredor>();
        services.AddHealthChecks().AddCheck<CorredorHealthCheck>("corredor", tags: ["ready"]);
        return services;
    }
    private static void RegistrarLecturasYTareas(IServiceCollection services)
    {
        services.AddScoped<ICaducidadRepository, CaducidadRepository>();
        services.AddScoped<IPurgasRepository, PurgasRepository>();
        services.AddScoped<Application.Tareas.CaducarAnomaliasUseCase>();
        services.AddScoped<Application.Tareas.PurgarSeudonimosUseCase>();
        services.AddScoped<Application.Tareas.PurgarHistoricoUseCase>();
        services.AddHostedService<Infrastructure.Tareas.TareaCaducidad>();
        services.AddHostedService<Infrastructure.Tareas.TareaPurgaSeudonimos>();
        services.AddHostedService<Infrastructure.Tareas.TareaPurgaHistorica>();
        services.AddScoped<ILecturaAnomaliasRepository, LecturaAnomaliasRepository>();
        services.AddScoped<IResumenDiaRepository, ResumenDiaRepository>();
        services.AddScoped<IAnomaliaLectura, Application.Lecturas.AnomaliaLectura>();
    }
    private static void RegistrarAdministracion(IServiceCollection services)
    {
        services.AddScoped<IDetalleAnomaliaRepository, DetalleAnomaliaRepository>();
        services.AddScoped<ILecturaAdministracionRepository, LecturaAdministracionRepository>();
        services.AddScoped<Application.Administracion.ObtenerDetalleUseCase>();
        services.AddScoped<Application.Administracion.ListarPendientesUseCase>();
        services.AddScoped<Application.Administracion.ListarPorRevisarUseCase>();
        services.AddScoped<Application.Administracion.ListarTransicionesRecientesUseCase>();
        services.AddScoped<Application.Administracion.ModerarAnomaliaUseCase>();
        services.AddScoped<IExportacionRepository, ExportacionRepository>();
        services.AddScoped<Application.Exportacion.ExportarDatasetUseCase>();
        services.AddSingleton<Application.Exportacion.IEscritorExportacion, Infrastructure.Exportacion.EscritorCsv>();
        services.AddSingleton<Application.Exportacion.IEscritorExportacion, Infrastructure.Exportacion.EscritorGeoJson>();
    }
}
