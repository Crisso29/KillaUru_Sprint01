using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Contratos;

/// <summary>
/// T042 (FR-016, constitución IX): lo que la API publica coincide con los contratos de specs. Revisa las
/// features ya implementadas; cada feature nueva se agrega a <see cref="Implementadas"/> al terminarla.
/// </summary>
public sealed class ContratoOpenApiTests(BaseDatosPrueba bd)
{
    private static readonly string[] Implementadas = ["001-plataforma-backend/contracts/plataforma.openapi.yaml"];

    public static TheoryData<string> Contratos() => new(Implementadas);

    [Theory]
    [MemberData(nameof(Contratos))]
    public async Task Cada_operacion_del_contrato_existe_con_sus_codigos(string contrato)
    {
        using var api = new FabricaApi(bd);
        var generado = JsonDocument.Parse(await api.CreateClient().GetStringAsync("/openapi/v1.json", TestContext.Current.CancellationToken));
        var esperado = ContratoYaml.Leer(Path.Combine(Rutas.Specs, contrato));

        foreach (var operacion in esperado)
        {
            var nombre = $"{operacion.Metodo.ToUpperInvariant()} {operacion.Ruta}";
            Assert.True(generado.RootElement.GetProperty("paths").TryGetProperty(operacion.Ruta, out var ruta), $"Falta la ruta de {nombre}");
            Assert.True(ruta.TryGetProperty(operacion.Metodo, out var metodo), $"Falta el método de {nombre}");
            var codigos = metodo.GetProperty("responses").EnumerateObject().Select(r => r.Name).ToHashSet();
            Assert.Superset(operacion.Codigos.ToHashSet(), codigos);
        }
    }
}
