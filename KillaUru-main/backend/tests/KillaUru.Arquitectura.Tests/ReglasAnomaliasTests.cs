namespace KillaUru.Arquitectura.Tests;
/// <summary>Auditoría es el único dueño de las métricas de riesgo; la ingesta no audita reportes.</summary>
public sealed class ReglasAnomaliasTests
{
    [Fact]
    public void Anomalias_no_emite_metricas_de_riesgo()
    {
        var carpeta = Path.Combine(Repositorio.Raiz, "src", "Modules", "KillaUru.Anomalias");
        foreach (var archivo in Directory.EnumerateFiles(carpeta, "*.cs", SearchOption.AllDirectories))
            Assert.DoesNotContain("RegistrarAccionRiesgo", File.ReadAllText(archivo), StringComparison.Ordinal);
    }

    [Fact]
    public void Procesamiento_no_depende_de_auditoria()
    {
        var carpeta = Path.Combine(Repositorio.Raiz, "src", "Modules", "KillaUru.Anomalias", "Application", "Procesamiento");
        foreach (var archivo in Directory.EnumerateFiles(carpeta, "*.cs", SearchOption.AllDirectories))
            Assert.DoesNotContain("KillaUru.Auditoria", File.ReadAllText(archivo), StringComparison.Ordinal);
    }
}
