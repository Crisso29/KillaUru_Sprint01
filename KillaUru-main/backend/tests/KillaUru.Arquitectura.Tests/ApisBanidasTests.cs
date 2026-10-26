namespace KillaUru.Arquitectura.Tests;

/// <summary>
/// Spec 001 — FR-036 (T-QA16, constitución III, CHK036):
/// ningún archivo de producción lee el reloj del sistema directamente.
/// El analyzer Microsoft.CodeAnalysis.BannedApiAnalyzers lo prohíbe en tiempo de compilación;
/// este test verifica que la configuración del analyzer esté activa y sea completa,
/// y actúa como red de seguridad frente a archivos que el compiler no evalúa (scripts, SQL).
/// </summary>
public sealed class ApisBanidasTests
{
    // Símbolos que BannedSymbols.txt debe prohibir (constitución III, T-QA16)
    private static readonly string[] SimbolosProhibidos =
    [
        "P:System.DateTime.Now",
        "P:System.DateTime.UtcNow",
        "P:System.DateTime.Today",
        "P:System.DateTimeOffset.Now",
        "P:System.DateTimeOffset.UtcNow",
    ];

    [Fact]
    public void BannedSymbols_existe_y_contiene_todos_los_simbolos_prohibidos()
    {
        // Arrange — localizar el archivo de configuración del analyzer
        var ruta = Path.Combine(Repositorio.Raiz, "BannedSymbols.txt");

        // Act
        var contenido = File.ReadAllText(ruta);

        // Assert — cada símbolo prohibido debe estar declarado (FR-036, T-QA16)
        Assert.True(File.Exists(ruta),
            "BannedSymbols.txt no existe; sin él el analyzer no puede bloquear DateTime.Now");
        foreach (var simbolo in SimbolosProhibidos)
            Assert.Contains(simbolo, contenido, StringComparison.Ordinal);
    }

    [Fact]
    public void Directory_Build_props_activa_el_analyzer_y_apunta_a_BannedSymbols()
    {
        // Arrange — leer la configuración MSBuild compartida
        var ruta = Path.Combine(Repositorio.Raiz, "Directory.Build.props");

        // Act
        var contenido = File.ReadAllText(ruta);

        // Assert — el paquete y el archivo adicional deben estar declarados (FR-036)
        Assert.Contains("Microsoft.CodeAnalysis.BannedApiAnalyzers", contenido, StringComparison.Ordinal);
        Assert.Contains("BannedSymbols.txt", contenido, StringComparison.Ordinal);
    }

    [Fact]
    public void Ningun_archivo_de_produccion_llama_a_DateTime_Now_directamente()
    {
        // Arrange — archivos .cs de src/ (el código de producción compilado)
        var raiz = Repositorio.Raiz;
        var archivos = Directory.EnumerateFiles(
            Path.Combine(raiz, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Split(Path.DirectorySeparatorChar).Any(p => p is "bin" or "obj"));

        // Patrones de texto que indican una llamada directa al reloj del sistema
        string[] patrones = ["DateTime.Now", "DateTime.UtcNow", "DateTime.Today",
                             "DateTimeOffset.Now", "DateTimeOffset.UtcNow"];

        // Act + Assert — ningún archivo debe contener los patrones prohibidos (FR-036, T-QA16)
        foreach (var archivo in archivos)
        {
            var texto = File.ReadAllText(archivo);
            foreach (var patron in patrones)
                Assert.DoesNotContain(patron, texto, StringComparison.Ordinal);
        }
    }
}
