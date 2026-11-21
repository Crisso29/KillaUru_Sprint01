using System.Diagnostics;
using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;
namespace KillaUru.Integracion.Tests.Auditoria;

/// <summary>SC-004: mide HTTP completo con 50 000 registros; no sustituye el ensayo en la VM objetivo.</summary>
public sealed class RendimientoConsultaTests(BaseDatosPrueba bd)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Un_mes_con_cincuenta_mil_registros_tiene_p95_menor_a_500_ms()
    {
        var actor = Guid.NewGuid();
        await SemillasAuditoria.InsertarAsync(bd.CadenaApp, actor, 50_000);
        using var api = new FabricaApi(bd);
        using var cliente = api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", Tokens.Emitir("Administrador"));
        var ruta = $"/api/v1/auditoria?actorId={actor}&desde=2026-10-01T00:00:00Z&hasta=2026-10-31T23:59:59Z&tamano=50";
        for (var i = 0; i < 3; i++) await cliente.GetStringAsync(ruta, Ct);
        var duraciones = await MedirAsync(cliente, ruta);
        var p95 = duraciones.Order().ElementAt(28);
        var carpeta = Path.Combine(Rutas.Backend, "TestResults");
        Directory.CreateDirectory(carpeta);
        await File.WriteAllTextAsync(Path.Combine(carpeta, "auditoria-rendimiento.json"),
            JsonSerializer.Serialize(new { registros = 50_000, muestras = 30, p95Ms = p95 }), Ct);
        Assert.True(p95 < 500, $"p95 = {p95:F2} ms; meta < 500 ms.");
    }

    private static async Task<List<double>> MedirAsync(HttpClient cliente, string ruta)
    {
        var duraciones = new List<double>();
        for (var i = 0; i < 30; i++)
        {
            var inicio = Stopwatch.GetTimestamp();
            var texto = await cliente.GetStringAsync(ruta, Ct);
            duraciones.Add(Stopwatch.GetElapsedTime(inicio).TotalMilliseconds);
            using var pagina = JsonDocument.Parse(texto);
            Assert.Equal(50_000, pagina.RootElement.GetProperty("total").GetInt64());
            Assert.Equal(50, pagina.RootElement.GetProperty("elementos").GetArrayLength());
        }
        return duraciones;
    }
}
