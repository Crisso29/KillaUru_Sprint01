using YamlDotNet.RepresentationModel;

namespace KillaUru.Integracion.Tests.Contratos;

/// <summary>Lee de un contrato OpenAPI en YAML cada operación con su ruta, método y códigos de respuesta.</summary>
internal static class ContratoYaml
{
    private static readonly string[] Metodos = ["get", "post", "put", "patch", "delete"];

    internal sealed record Operacion(string Ruta, string Metodo, IReadOnlyList<string> Codigos);

    internal static IReadOnlyList<Operacion> Leer(string archivo)
    {
        var yaml = new YamlStream();
        using (var lector = new StreamReader(archivo))
        {
            yaml.Load(lector);
        }

        var raiz = (YamlMappingNode)yaml.Documents[0].RootNode;
        var rutas = (YamlMappingNode)raiz.Children[new YamlScalarNode("paths")];
        return rutas.Children
            .SelectMany(ruta => ((YamlMappingNode)ruta.Value).Children
                .Where(m => Metodos.Contains(m.Key.ToString()))
                .Select(m => new Operacion(ruta.Key.ToString(), m.Key.ToString(), Codigos((YamlMappingNode)m.Value))))
            .ToList();
    }

    private static List<string> Codigos(YamlMappingNode operacion) =>
        ((YamlMappingNode)operacion.Children[new YamlScalarNode("responses")]).Children.Keys.Select(k => k.ToString()).ToList();
}
