using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KillaUru.Arquitectura.Tests;

/// <summary>
/// Spec 001 — FR-029 (D17, D18, CHK035):
/// revisa sintaxis real con Roslyn para garantizar legibilidad: un tipo por archivo,
/// sin clases parciales y métodos de máximo 20 líneas.
/// Partial se prohíbe porque fragmenta la responsabilidad en archivos invisibles entre sí.
/// </summary>
public sealed class LegibilidadTests
{
    /// <summary>
    /// Enumera todos los .cs de src/ salvo los artefactos de compilación (bin/, obj/).
    /// Se usa como TheoryData para ejecutar la regla archivo por archivo.
    /// </summary>
    public static TheoryData<string> Fuentes() => new(
        Directory.EnumerateFiles(Path.Combine(Repositorio.Raiz, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(r => !r.Split(Path.DirectorySeparatorChar).Any(p => p is "bin" or "obj")));

    [Theory, MemberData(nameof(Fuentes))]
    public void Cada_archivo_tiene_un_tipo_sin_partial_y_metodos_cortos(string archivo)
    {
        // Arrange — parsear el árbol de sintaxis con Roslyn para inspeccionar la estructura real
        var ct   = TestContext.Current.CancellationToken;
        var arbol = CSharpSyntaxTree.ParseText(File.ReadAllText(archivo), cancellationToken: ct).GetRoot(ct);
        var tipos = arbol.DescendantNodes().OfType<BaseTypeDeclarationSyntax>().ToArray();

        // Act + Assert — un tipo por archivo (D17), sin partial (D18)
        Assert.True(tipos.Length <= 1,
            $"{archivo}: se encontraron {tipos.Length} tipos; máximo 1 por archivo");
        Assert.DoesNotContain(tipos,
            t => t.Modifiers.Any(SyntaxKind.PartialKeyword));

        // Assert — cada método tiene máximo 20 líneas (D17: Single Responsibility)
        foreach (var metodo in arbol.DescendantNodes().OfType<BaseMethodDeclarationSyntax>())
        {
            var cuerpo = (SyntaxNode?)metodo.Body ?? metodo.ExpressionBody;
            if (cuerpo is null) continue;
            var span  = cuerpo.GetLocation().GetLineSpan();
            var total = span.EndLinePosition.Line - span.StartLinePosition.Line + 1;
            Assert.True(total <= 20,
                $"{archivo}: método en línea {span.StartLinePosition.Line + 1} tiene {total} líneas (máximo 20)");
        }
    }
}
