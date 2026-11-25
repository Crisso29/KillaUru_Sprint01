using Microsoft.Extensions.Options;
using Npgsql;
using Npgsql.NetTopologySuite;

namespace KillaUru.Api.Configuracion;

/// <summary>Opciones del host (validadas al arrancar) y la fuente de conexiones a PostgreSQL.</summary>
internal static class ConfiguracionExtensions
{
    private const string NombreConexion = "KillaUru";

    internal static IServiceCollection AgregarConfiguracion(this IServiceCollection services, IConfiguration configuracion)
    {
        services.AddOptions<ApiOptions>().BindConfiguration(ApiOptions.Seccion).ValidateOnStart();
        services.AddSingleton<IValidateOptions<ApiOptions>, ValidadorApiOptions>();

        // Un único NpgsqlDataSource para toda la API (DT-02): un solo pool de 30 conexiones
        // en vez de 3 × 30 = 90. NetTopologySuite habilitado aquí para Anomalías (PostGIS).
        // La API se conecta como killauru_app (solo DML); nunca con el rol que migra (RNF-18).
        var cadena = configuracion.GetConnectionString(NombreConexion)
            ?? throw new InvalidOperationException($"Falta ConnectionStrings:{NombreConexion}.");
        var builder = new NpgsqlDataSourceBuilder(cadena);
        builder.UseNetTopologySuite();
        services.AddSingleton(builder.Build());
        return services;
    }
}
