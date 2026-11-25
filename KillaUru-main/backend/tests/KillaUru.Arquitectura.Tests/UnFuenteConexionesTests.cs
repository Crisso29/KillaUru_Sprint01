namespace KillaUru.Arquitectura.Tests;

/// <summary>
/// DT-02 (001/T082): un único NpgsqlDataSource registrado en el contenedor, compartido por todos
/// los DbContext. Antes había tres pools (N×30 = 90 conexiones); ahora uno solo (30 máx).
/// Los módulos reciben la fuente del DI; solo ConfiguracionExtensions la construye.
/// </summary>
public sealed class UnFuenteConexionesTests
{
    private static IEnumerable<string> ArchivosModulos() =>
        Directory.EnumerateFiles(
            Path.Combine(Repositorio.Raiz, "src", "Modules"), "*.cs", SearchOption.AllDirectories)
        .Where(f => !f.Split(Path.DirectorySeparatorChar).Any(p => p is "bin" or "obj"));

    /// <summary>Ningún módulo crea su propio NpgsqlDataSource: ese rol es exclusivo de ConfiguracionExtensions.</summary>
    [Fact]
    public void Ningun_modulo_crea_su_propio_NpgsqlDataSource()
    {
        // Arrange + Act + Assert
        foreach (var archivo in ArchivosModulos())
        {
            var texto = File.ReadAllText(archivo);
            Assert.DoesNotContain("NpgsqlDataSource.Create(", texto, StringComparison.Ordinal);
            Assert.DoesNotContain("new NpgsqlDataSourceBuilder(", texto, StringComparison.Ordinal);
        }
    }

    /// <summary>Ningún módulo registra su DbContext con una cadena de conexión literal: usa el DataSource del DI.</summary>
    [Fact]
    public void Ningun_modulo_registra_DbContext_con_cadena_literal()
    {
        // Arrange + Act + Assert
        foreach (var archivo in ArchivosModulos())
        {
            var texto = File.ReadAllText(archivo);
            Assert.DoesNotContain("GetConnectionString(", texto, StringComparison.Ordinal);
        }
    }

    /// <summary>ConfiguracionExtensions construye la fuente con NetTopologySuite habilitado (Anomalías necesita PostGIS).</summary>
    [Fact]
    public void ConfiguracionExtensions_construye_fuente_unica_con_NetTopologySuite()
    {
        // Arrange
        var archivo = Path.Combine(Repositorio.Raiz, "src", "KillaUru.Api",
            "Configuracion", "ConfiguracionExtensions.cs");

        // Act
        var texto = File.ReadAllText(archivo);

        // Assert — NpgsqlDataSourceBuilder con NTS (DT-02)
        Assert.Contains("NpgsqlDataSourceBuilder", texto, StringComparison.Ordinal);
        Assert.Contains("UseNetTopologySuite", texto, StringComparison.Ordinal);
        Assert.DoesNotContain("NpgsqlDataSource.Create(", texto, StringComparison.Ordinal);
    }
}
