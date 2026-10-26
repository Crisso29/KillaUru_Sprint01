using System.Net.Http.Json;
using System.Text.Json;
using KillaUru.Auditoria.Contracts;
using KillaUru.Auditoria.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>El conteo auditado coincide con el archivo entregado en ambos formatos.</summary>
[Collection("Anomalias")]
public sealed class ExportacionApiTests(BaseDatosPrueba bd)
{
    private static readonly string[] Activas = ["Activa"];
    [Theory]
    [InlineData("CSV")]
    [InlineData("GeoJSON")]
    public async Task Exporta_y_audita_el_mismo_numero_de_filas(string formato)
    {
        using var e = new EscenarioAnomalias(bd); await e.ProcesarAsync(e.Evento());
        using var cliente = e.Api.CreateClient(); var actor = Guid.NewGuid(); var ct = TestContext.Current.CancellationToken;
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", Tokens.Emitir("Administrador", actor.ToString(), reloj:e.Reloj));
        using var respuesta = await cliente.PostAsJsonAsync("/api/v1/exportaciones", new { formato, tipo="Bache", estados=Activas }, ct);
        respuesta.EnsureSuccessStatusCode(); Assert.Equal("attachment", respuesta.Content.Headers.ContentDisposition!.DispositionType);
        var texto = await respuesta.Content.ReadAsStringAsync(ct);
        Assert.DoesNotContain("seudonimo", texto, StringComparison.OrdinalIgnoreCase);
        var cantidad = formato == "CSV" ? texto.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length-1 : ContarGeoJson(texto);
        Assert.True(cantidad > 0);
        await using var scope = e.Api.Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<AuditoriaDbContext>();
        var registro = await db.Registros.SingleAsync(r => r.ActorId == actor && r.Accion == (short)AccionAuditada.DatasetExportado, ct);
        using var contexto = JsonDocument.Parse(registro.Contexto);
        Assert.Equal(cantidad, contexto.RootElement.GetProperty("filas").GetInt32());
    }
    private static int ContarGeoJson(string texto)
    {
        using var json = JsonDocument.Parse(texto);
        return json.RootElement.GetProperty("features").GetArrayLength();
    }
}
