namespace KillaUru.Integracion.Tests.Infraestructura;

/// <summary>Rutas del repositorio que usan las pruebas (la raíz es la carpeta con KillaUru.sln).</summary>
internal static class Rutas
{
    internal static string Backend { get; } = BuscarBackend();

    internal static string Migraciones => Path.Combine(Backend, "db", "migraciones");

    /// <summary>Contratos OpenAPI de todas las features (specs/*/contracts).</summary>
    internal static string Specs => Path.Combine(Directory.GetParent(Backend)!.FullName, "specs");

    /// <summary>Scripts V0001… en orden de versión (V0000 se aplica aparte, como superusuario).</summary>
    /// <returns>Rutas locales ordenadas.</returns>
    internal static IEnumerable<string> MigracionesVersionadas() =>
        Directory.EnumerateFiles(Migraciones, "V*.sql")
            .Where(f => !Path.GetFileName(f).StartsWith("V0000", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal);

    private static string BuscarBackend()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "KillaUru.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("No se encontró KillaUru.sln");
    }
}
