using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KillaUru.Arquitectura.Tests;

/// <summary>Revisa sintaxis real: métodos cortos, una responsabilidad por archivo y ausencia de partial.</summary>
public sealed class LegibilidadTests
{
    public static TheoryData<string> Fuentes() => new(Directory.EnumerateFiles(
        Path.Combine(Repositorio.Raiz, "src"), "*.cs", SearchOption.AllDirectories)
        .Where(r => !r.Split(Path.DirectorySeparatorChar).Any(p => p is "bin" or "obj")));

    [Theory, MemberData(nameof(Fuentes))]
    public void Cada_archivo_tiene_un_tipo_sin_partial_y_metodos_cortos(string archivo)
    {
        var ct = TestContext.Current.CancellationToken;
        var raiz = CSharpSyntaxTree.ParseText(File.ReadAllText(archivo), cancellationToken: ct).GetRoot(ct);
        var tipos = raiz.DescendantNodes().OfType<BaseTypeDeclarationSyntax>().ToArray();
        Assert.True(tipos.Length <= 1, $"{archivo}: un archivo por tipo, incluidos los anidados.");
        Assert.DoesNotContain(tipos, t => t.Modifiers.Any(SyntaxKind.PartialKeyword));
        foreach (var metodo in raiz.DescendantNodes().OfType<BaseMethodDeclarationSyntax>())
        {
            var cuerpo = (SyntaxNode?)metodo.Body ?? metodo.ExpressionBody;
            if (cuerpo is null) continue;
            var lineas = cuerpo.GetLocation().GetLineSpan();
            var total = lineas.EndLinePosition.Line - lineas.StartLinePosition.Line + 1;
            Assert.True(total <= 20, $"{archivo}: método en línea {lineas.StartLinePosition.Line + 1}, {total} líneas (máximo 20).");
        }
    }
}
