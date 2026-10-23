using Microsoft.Extensions.Options;
using Npgsql;

namespace KillaUru.Api.Configuracion;

/// <summary>Opciones del host (validadas al arrancar) y la fuente de conexiones a PostgreSQL.</summary>
internal static class ConfiguracionExtensions
{
    private const string NombreConexion = "KillaUru";

    internal static IServiceCollection AgregarConfiguracion(this IServiceCollection services, IConfiguration configuracion)
    {
        services.AddOptions<ApiOptions>().BindConfiguration(ApiOptions.Seccion).ValidateOnStart();
        services.AddSingleton<IValidateOptions<ApiOptions>, ValidadorApiOptions>();

        // La API se conecta como killauru_app (solo DML); nunca con el rol que migra (RNF-18).
        var cadena = configuracion.GetConnectionString(NombreConexion)
            ?? throw new InvalidOperationException($"Falta ConnectionStrings:{NombreConexion}.");
        services.AddSingleton(_ => NpgsqlDataSource.Create(cadena));
        return services;
    }
}
