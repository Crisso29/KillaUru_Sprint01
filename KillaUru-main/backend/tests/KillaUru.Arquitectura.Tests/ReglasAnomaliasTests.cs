namespace KillaUru.Arquitectura.Tests;

/// <summary>
/// Auditoría es la única dueña de las métricas de riesgo, y la ingesta de reportes no audita: un
/// descarte es una métrica, no una acción de una persona. Spec 004 · FR-005, FR-031.
/// </summary>
public sealed class ReglasAnomaliasTests
{
    private static string CarpetaDe(params string[] partes) =>
        Path.Combine([Repositorio.Raiz, "src", "Modules", "KillaUru.Anomalias", .. partes]);

    private static IEnumerable<string> ArchivosQueMencionan(string carpeta, string texto) =>
        Directory.EnumerateFiles(carpeta, "*.cs", SearchOption.AllDirectories)
            .Where(archivo => File.ReadAllText(archivo).Contains(texto, StringComparison.Ordinal));

    /// <summary>Ningún archivo de Anomalías llama RegistrarAccionRiesgo (FR-031).</summary>
    [Fact]
    public void Anomalias_no_emite_metricas_de_riesgo()
    {
        // Arrange
        var modulo = CarpetaDe();

        // Act
        var infractores = ArchivosQueMencionan(modulo, "RegistrarAccionRiesgo");

        // Assert
        Assert.Empty(infractores);
    }

    /// <summary>El procesamiento de reportes no depende de Auditoría: los descartes no se auditan (FR-005).</summary>
    [Fact]
    public void Procesamiento_no_depende_de_auditoria()
    {
        // Arrange
        var procesamiento = CarpetaDe("Application", "Procesamiento");

        // Act
        var infractores = ArchivosQueMencionan(procesamiento, "KillaUru.Auditoria");

        // Assert
        Assert.Empty(infractores);
    }
}
