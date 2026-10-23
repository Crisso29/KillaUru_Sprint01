using System.Text.Json;
using YamlDotNet.RepresentationModel;
namespace KillaUru.Integracion.Tests.Contratos;

/// <summary>Compara restricciones del YAML con OpenAPI generado, resolviendo referencias locales.</summary>
internal sealed class ComparadorEsquemas(YamlMappingNode esperado, JsonElement generado)
{
    internal void Comparar(YamlMappingNode origen, JsonElement destino)
    {
        origen = Resolver(origen);
        destino = Resolver(destino);
        foreach (var nombre in new[] { "type", "format", "enum", "required", "minimum", "maximum", "maxLength", "default" })
            CompararRestriccion(origen, destino, nombre);
        if (origen.Children.TryGetValue("properties", out var propiedades))
        {
            foreach (var (clave, valor) in ((YamlMappingNode)propiedades).Children)
                Comparar((YamlMappingNode)valor, destino.GetProperty("properties").GetProperty(clave.ToString()));
        }
        if (origen.Children.TryGetValue("items", out var items))
            Comparar((YamlMappingNode)items, destino.GetProperty("items"));
    }

    private static void CompararRestriccion(YamlMappingNode origen, JsonElement destino, string nombre)
    {
        if (!origen.Children.TryGetValue(nombre, out var valor)) return;
        Assert.True(destino.TryGetProperty(nombre, out var actual), $"Falta la restricción {nombre} en {destino}");
        if (valor is YamlSequenceNode secuencia)
        {
            var valores = secuencia.Children.Select(x => x.ToString()).Order().ToArray();
            Assert.Equal(valores, actual.EnumerateArray().Select(x => x.ToString()).Order());
        }
        else
        {
            Assert.Equal(valor.ToString(), actual.ToString());
        }
    }

    private YamlMappingNode Resolver(YamlMappingNode esquema) =>
        esquema.Children.TryGetValue("$ref", out var referencia)
            ? (YamlMappingNode)((YamlMappingNode)((YamlMappingNode)esperado.Children["components"]).Children["schemas"])
                .Children[referencia.ToString().Split('/')[^1]]
            : esquema;

    private JsonElement Resolver(JsonElement esquema) =>
        esquema.TryGetProperty("$ref", out var referencia)
            ? generado.GetProperty("components").GetProperty("schemas").GetProperty(referencia.GetString()!.Split('/')[^1])
            : esquema;
}
