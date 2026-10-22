using System.Text.RegularExpressions;

namespace KillaUru.Arquitectura.Tests;

/// <summary>Ubica archivos del backend en disco (la raíz es la carpeta que contiene KillaUru.sln).</summary>
internal static class Repositorio
{
    private static readonly Regex ReferenciaAModulo =
        new(@"ProjectReference Include=""[^""]*KillaUru\.(\w+)\.csproj""", RegexOptions.Compiled);

    internal static string Raiz { get; } = BuscarRaiz();

    /// <summary>Nombres de los módulos que el .csproj del módulo referencia (el grafo declarado).</summary>
    internal static IEnumerable<string> ModulosReferenciados(string modulo)
    {
        var csproj = Path.Combine(Raiz, "src", "Modules", $"KillaUru.{modulo}", $"KillaUru.{modulo}.csproj");
        return ReferenciaAModulo.Matches(File.ReadAllText(csproj)).Select(m => m.Groups[1].Value);
    }

    private static string BuscarRaiz()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "KillaUru.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("No se encontró KillaUru.sln");
    }
}
