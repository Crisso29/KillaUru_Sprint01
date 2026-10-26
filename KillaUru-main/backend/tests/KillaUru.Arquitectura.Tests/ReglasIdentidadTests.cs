namespace KillaUru.Arquitectura.Tests;

/// <summary>
/// Spec 001 — FR-029 (D15, CHK035):
/// las métricas de riesgo solo se emiten desde el módulo de Auditoría, después de confirmar
/// la transacción. Identidad no debe emitirlas porque eso violaría la separación de contextos
/// y podría registrar métricas sobre operaciones que luego se revierten.
/// </summary>
public sealed class ReglasIdentidadTests
{
    [Fact]
    public void Identidad_no_emite_metricas_de_riesgo()
    {
        // Arrange — apuntar a la carpeta Application del módulo de Identidad
        var carpeta = Path.Combine(
            Repositorio.Raiz, "src", "Modules", "KillaUru.Identidad", "Application");

        // Act + Assert — ningún archivo .cs contiene la llamada prohibida (D15)
        foreach (var archivo in Directory.EnumerateFiles(carpeta, "*.cs", SearchOption.AllDirectories))
        {
            Assert.DoesNotContain("RegistrarAccionRiesgo", File.ReadAllText(archivo),
                StringComparison.Ordinal);
        }
    }
}
