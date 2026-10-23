using Testcontainers.PostgreSql;

namespace KillaUru.Integracion.Tests.Infraestructura;

/// <summary>
/// PostgreSQL + PostGIS real (Testcontainers), una sola vez por ejecución de pruebas. Se prepara igual
/// que producción: V0000 como superusuario y luego cada migración como killauru_migrador (T-BD01).
/// La API de pruebas se conecta como killauru_app, el rol de solo DML.
/// </summary>
public sealed class BaseDatosPrueba : IAsyncLifetime
{
    internal const string ClaveSuperusuario = "super-prueba";
    internal const string ClaveMigrador = "migrador-prueba";
    internal const string ClaveApp = "app-prueba";
    private const string Imagen = "postgis/postgis:17-3.5";

    private readonly PostgreSqlContainer _contenedor = new PostgreSqlBuilder(Imagen)
        .WithDatabase("killauru")
        .WithUsername("postgres")
        .WithPassword(ClaveSuperusuario)
        .WithResourceMapping(new DirectoryInfo(Rutas.Migraciones), MigradorPrueba.CarpetaEnContenedor)
        .Build();

    /// <summary>Aplica scripts SQL dentro del contenedor, como lo hace migrar.sh.</summary>
    internal MigradorPrueba Migrador => new(_contenedor);

    /// <summary>Conexión como killauru_app (lo que usa la API).</summary>
    public string CadenaApp => Cadena("killauru_app", ClaveApp);

    /// <summary>Conexión como killauru_migrador (DDL).</summary>
    public string CadenaMigrador => Cadena("killauru_migrador", ClaveMigrador);

    /// <summary>Conexión como superusuario (solo para verificaciones).</summary>
    public string CadenaSuperusuario => _contenedor.GetConnectionString();

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await _contenedor.StartAsync();
        await Migrador.AplicarArranqueAsync();
        foreach (var archivo in Rutas.MigracionesVersionadas())
        {
            await Migrador.AplicarAsync(archivo);
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => _contenedor.DisposeAsync();

    private string Cadena(string usuario, string clave) =>
        $"Host={_contenedor.Hostname};Port={_contenedor.GetMappedPublicPort(5432)};Database=killauru;Username={usuario};Password={clave}";
}
