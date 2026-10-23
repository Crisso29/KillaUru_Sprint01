namespace KillaUru.Arquitectura.Tests;

/// <summary>
/// Constitución VIII (D19): ningún archivo de código llega a 200 líneas. Si crece, se extrae otra clase
/// con una sola responsabilidad. Excepciones: migraciones versionadas, código generado y contratos OpenAPI.
/// </summary>
public sealed class LimiteDeLineasTests
{
    private const int LineasMaximas = 199;

    private static readonly (string Carpeta, string Patron)[] Revisados =
    [
        ("src", "*.cs"), ("tests", "*.cs"), ("db", "*.sql"), ("deploy", "*.sh"), (Path.Combine("tests", "carga"), "*.js"),
        ("deploy", "*.py"),
    ];

    [Fact]
    public void Ningun_archivo_de_codigo_llega_a_200_lineas()
    {
        var raiz = Repositorio.Raiz;

        var excedidos = Revisados
            .Where(r => Directory.Exists(Path.Combine(raiz, r.Carpeta)))
            .SelectMany(r => Directory.EnumerateFiles(Path.Combine(raiz, r.Carpeta), r.Patron, SearchOption.AllDirectories))
            .Where(EsRevisable)
            .Select(f => (Archivo: Path.GetRelativePath(raiz, f), Lineas: File.ReadLines(f).Count()))
            .Where(f => f.Lineas > LineasMaximas)
            .Select(f => $"{f.Archivo} ({f.Lineas})")
            .ToList();

        Assert.True(excedidos.Count == 0, $"Archivos con 200 líneas o más: {string.Join(", ", excedidos)}");
    }

    private static bool EsRevisable(string ruta)
    {
        var nombre = Path.GetFileName(ruta);
        var partes = ruta.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var esMigracion = partes.Contains("migraciones") && nombre.StartsWith('V');
        var esGenerado = partes.Contains("obj") || partes.Contains("bin") || nombre.EndsWith(".g.cs", StringComparison.Ordinal);
        return !esMigracion && !esGenerado;
    }
}
