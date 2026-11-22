using System.Text.Json;
using System.Text.RegularExpressions;

namespace KillaUru.Arquitectura.Tests;

/// <summary>
/// Decisión de `diseño-interno-de-modulos.md` §3.7: un pool de Npgsql de **30** conexiones como máximo.
/// Sin el tope, Npgsql abre hasta 100 y PostgreSQL (max_connections 100, 3 reservadas) rechaza con 53300:
/// bajo una ráfaga de 1 200 usuarios la recepción respondía 500 (k6, 2026-11-22; ARQ-22).
/// Se comprueba cada cadena de conexión que usa la API: la base y la de cada compose, que la reemplaza.
/// </summary>
public sealed class PoolDeConexionesTests
{
    private const int MaximoDocumentado = 30;

    private static readonly Regex CadenaEnCompose =
        new(@"ConnectionStrings__KillaUru:\s*""(?<cadena>[^""]+)""", RegexOptions.Compiled);

    public static TheoryData<string> Compose() =>
        new("docker-compose.yml", "docker-compose.pruebas.yml", "docker-compose.local.yml");

    private static int? MaximoDelPool(string cadena) =>
        cadena.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(par => par.Split('=', 2))
            .Where(par => par.Length == 2 && par[0].Trim().Replace(" ", "").Equals("MaximumPoolSize", StringComparison.OrdinalIgnoreCase))
            .Select(par => (int?)int.Parse(par[1].Trim(), System.Globalization.CultureInfo.InvariantCulture))
            .SingleOrDefault();

    /// <summary>La cadena base de `appsettings.json` lleva el tope documentado.</summary>
    [Fact]
    public void Appsettings_limita_el_pool_a_30_conexiones()
    {
        // Arrange
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(Repositorio.Raiz, "src", "KillaUru.Api", "appsettings.json")));

        // Act
        var cadena = json.RootElement.GetProperty("ConnectionStrings").GetProperty("KillaUru").GetString()!;

        // Assert
        Assert.Equal(MaximoDocumentado, MaximoDelPool(cadena));
    }

    /// <summary>Cada compose reemplaza la cadena completa: también debe llevar el tope, o se pierde en el despliegue.</summary>
    [Theory]
    [MemberData(nameof(Compose))]
    public void Compose_limita_el_pool_a_30_conexiones(string archivo)
    {
        // Arrange
        var contenido = File.ReadAllText(Path.Combine(Repositorio.Raiz, "deploy", archivo));

        // Act
        var cadena = CadenaEnCompose.Match(contenido).Groups["cadena"].Value;

        // Assert
        Assert.False(string.IsNullOrEmpty(cadena), $"{archivo} no define ConnectionStrings__KillaUru");
        Assert.Equal(MaximoDocumentado, MaximoDelPool(cadena));
    }
}
