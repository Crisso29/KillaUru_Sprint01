using System.Reflection;

namespace KillaUru.Arquitectura.Tests;

/// <summary>
/// Fuente única de los módulos y del grafo de dependencias permitido
/// (componentes-arquitectonicos.md, regla 4). Las pruebas lo leen; nadie lo duplica.
/// </summary>
internal static class Modulos
{
    internal static readonly IReadOnlyDictionary<string, Assembly> Ensamblados = new Dictionary<string, Assembly>
    {
        ["Auditoria"] = typeof(KillaUru.Auditoria.AuditoriaModule).Assembly,
        ["Identidad"] = typeof(KillaUru.Identidad.IdentidadModule).Assembly,
        ["Anomalias"] = typeof(KillaUru.Anomalias.AnomaliasModule).Assembly,
        ["Ingesta"] = typeof(KillaUru.Ingesta.IngestaModule).Assembly,
        ["Mapa"] = typeof(KillaUru.Mapa.MapaModule).Assembly,
        ["Dashboard"] = typeof(KillaUru.Dashboard.DashboardModule).Assembly,
    };

    internal static readonly IReadOnlyDictionary<string, string[]> GrafoPermitido = new Dictionary<string, string[]>
    {
        ["Auditoria"] = [],
        ["Identidad"] = ["Auditoria"],
        ["Anomalias"] = ["Auditoria"],
        ["Ingesta"] = ["Anomalias"],
        ["Mapa"] = ["Anomalias"],
        ["Dashboard"] = ["Anomalias", "Identidad"],
    };

    internal static IEnumerable<string> Nombres => Ensamblados.Keys;

    internal static string Raiz(string modulo) => $"KillaUru.{modulo}";

    internal static string Capa(string modulo, string capa) => $"{Raiz(modulo)}.{capa}";

    internal static IEnumerable<string> Otros(string modulo) => Nombres.Where(n => n != modulo);

    public static TheoryData<string> Todos()
    {
        var datos = new TheoryData<string>();
        foreach (var nombre in Nombres)
        {
            datos.Add(nombre);
        }

        return datos;
    }
}
