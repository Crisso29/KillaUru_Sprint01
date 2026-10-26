using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Contratos;

/// <summary>
/// Parte de T042 (FR-016, constitución IX): verifica rutas, métodos y presencia de códigos de respuesta.
/// Solo revisa los contratos enumerados en <see cref="Implementadas"/>; no descubre otros automáticamente.
/// </summary>
/// <remarks>No compara todavía schemas, propiedades requeridas, ejemplos ni esquemas de seguridad.</remarks>
public sealed class ContratoOpenApiTests(BaseDatosPrueba bd)
{
    private static readonly string[] Implementadas =
        ["001-plataforma-backend/contracts/plataforma.openapi.yaml", "002-modulo-auditoria/contracts/auditoria.openapi.yaml",
        "003-modulo-identidad-accesos/contracts/identidad.openapi.yaml"];

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
