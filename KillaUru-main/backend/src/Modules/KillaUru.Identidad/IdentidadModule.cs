using KillaUru.Identidad.Adapters.Consola;
using KillaUru.Identidad.Adapters.Documentacion;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Cuenta;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Application.Sesion;
using KillaUru.Identidad.Application.Usuarios;
using KillaUru.Identidad.Contracts;
using KillaUru.Identidad.Infrastructure;
using KillaUru.Identidad.Infrastructure.Correo;
using KillaUru.Identidad.Infrastructure.Repositorios;
using KillaUru.Identidad.Infrastructure.Seguridad;
using KillaUru.Identidad.Infrastructure.Tareas;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace KillaUru.Identidad;

/// <summary>Raíz de composición de Identidad; expone únicamente contratos y el comando inicial.</summary>
public static class IdentidadModule
{
    /// <summary>Registra persistencia, seguridad, casos de uso y retención.</summary>
    /// <param name="services">Contenedor del host.</param>
    /// <param name="configuration">Configuración y firma compartida con el host.</param>
    /// <returns>El contenedor.</returns>
    public static IServiceCollection AddIdentidadModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<IdentidadOptions>().Bind(configuration.GetSection("Identidad")).ValidateOnStart();
        services.AddSingleton<IValidateOptions<IdentidadOptions>, ValidadorIdentidadOptions>();
        services.AddOptions<OpcionesFirmaJwt>().Bind(configuration.GetSection("Api:Jwt"));
        services.AddDbContext<IdentidadDbContext>(o => o.UseNpgsql(configuration.GetConnectionString("KillaUru")));
        RegistrarPersistencia(services);
        RegistrarSeguridad(services);
        RegistrarAplicacion(services);
        services.AddHostedService<PurgaIdentidadTarea>();
        RegistrarDocumentacion(services);
        return services;
    }

    /// <summary>Ejecuta crear-admin sin iniciar HTTP ni emitir el enlace por ILogger.</summary>
    /// <param name="services">Servicios construidos.</param>
    /// <param name="correo">Correo del primer administrador.</param>
    /// <param name="salida">Consola privada.</param>
    /// <param name="ct">Cancelación.</param>
    /// <returns>Cero al crear; uno si se rechaza.</returns>
    public static async Task<int> CrearAdministradorInicialAsync(IServiceProvider services, string correo, TextWriter salida, CancellationToken ct)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<CrearAdminComando>().EjecutarAsync(correo, salida, ct);
    }

    private static void RegistrarPersistencia(IServiceCollection services)
    {
        services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
        services.AddScoped<ISesionRepositorio, SesionRepositorio>();
        services.AddScoped<ITokenCorreoRepositorio, TokenCorreoRepositorio>();
        services.AddScoped<IRevisionRepositorio, RevisionRepositorio>();
        services.AddScoped<IPurgaIdentidadRepositorio, PurgaIdentidadRepositorio>();
        services.AddScoped<ITransaccionIdentidad, TransaccionIdentidad>();
    }

    private static void RegistrarDocumentacion(IServiceCollection services) => services.AddOpenApi("v1", opciones =>
    {
        var anterior = opciones.CreateSchemaReferenceId;
        opciones.CreateSchemaReferenceId = info => info.Type == typeof(SesionEmitida) ? "SesionApp"
            : info.Type == typeof(UsuarioVista) ? "Usuario" : anterior(info);
        opciones.AddSchemaTransformer<EsquemasIdentidad>().AddOperationTransformer<OperacionIdentidad>();
    });

    private static void RegistrarSeguridad(IServiceCollection services)
    {
        services.AddSingleton<IHashContrasenas, HashContrasenas>();
        services.AddSingleton<IGeneradorSecretos, GeneradorSecretos>();
        services.AddSingleton<IEmisorJwt, EmisorJwt>();
        services.AddSingleton<ILimitadorRecuperacion, LimitadorRecuperacionEnMemoria>();
        services.AddHttpClient<IVerificadorAntiBot, VerificadorTurnstile>(c => c.Timeout = TimeSpan.FromSeconds(10));
        services.AddHttpClient<EnviadorCorreoHttp>(c => c.Timeout = TimeSpan.FromSeconds(10));
        services.AddSingleton<EnviadorCorreoConsola>();
        services.AddScoped<IEnviadorCorreo>(sp =>
            sp.GetRequiredService<IHostEnvironment>().IsDevelopment()
            && sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<IdentidadOptions>>().Value.CorreoConsola
                ? sp.GetRequiredService<EnviadorCorreoConsola>() : sp.GetRequiredService<EnviadorCorreoHttp>());
    }

    private static void RegistrarAplicacion(IServiceCollection services)
    {
        services.AddScoped<AuditarIdentidad>();
        services.AddScoped<EmitirEnlaceCorreo>();
        services.AddScoped<ResolverEnlace>();
        services.AddScoped<EmisionSesion>();
        services.AddScoped<ProteccionAdministradores>();
        services.AddScoped<CrearAdminComando>();
        services.AddScoped<IIdentidadConsultas, IdentidadConsultas>();
        foreach (var tipo in typeof(IdentidadModule).Assembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith("KillaUru.Identidad.Application", StringComparison.Ordinal) == true
                && t.Name.EndsWith("UseCase", StringComparison.Ordinal))) { services.AddScoped(tipo); }
    }
}
