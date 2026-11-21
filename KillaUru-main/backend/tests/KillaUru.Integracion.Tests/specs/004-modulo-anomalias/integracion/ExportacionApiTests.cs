using System.Net.Http.Json;
using System.Text.Json;
using KillaUru.Auditoria.Contracts;
using KillaUru.Auditoria.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// Exportación por HTTP: el archivo se entrega como adjunto, sin seudónimos, y el número de filas
/// auditado coincide con el entregado en ambos formatos. Spec 004 · FR-028.
/// </summary>
[Collection("Anomalias")]
public sealed class ExportacionApiTests(BaseDatosPrueba bd)
{
    private static readonly string[] SoloActivas = ["Activa"];

    [Theory]
    [InlineData("CSV")]
    [InlineData("GeoJSON")]
    public async Task Exporta_y_audita_el_mismo_numero_de_filas(string formato)
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        await escenario.ProcesarAsync(escenario.Evento());
        var actor = Guid.NewGuid();
        using var cliente = escenario.ClienteComo("Administrador", actor);

        // Act
        using var respuesta = await cliente.PostAsJsonAsync("/api/v1/exportaciones", new { formato, tipo = "Bache", estados = SoloActivas }, EscenarioAnomalias.Ct);

        // Assert
        respuesta.EnsureSuccessStatusCode();
        Assert.Equal("attachment", respuesta.Content.Headers.ContentDisposition!.DispositionType);
        var archivo = await respuesta.Content.ReadAsStringAsync(EscenarioAnomalias.Ct);
        Assert.DoesNotContain("seudonimo", archivo, StringComparison.OrdinalIgnoreCase);
        var filasEntregadas = formato == "CSV" ? ContarFilasCsv(archivo) : ContarFeatures(archivo);
        Assert.True(filasEntregadas > 0);
        Assert.Equal(filasEntregadas, await FilasAuditadasAsync(escenario, actor));
    }

    /// <summary>Filas sin contar la cabecera.</summary>
    private static int ContarFilasCsv(string archivo) => archivo.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length - 1;

    private static int ContarFeatures(string archivo)
    {
        using var json = JsonDocument.Parse(archivo);
        return json.RootElement.GetProperty("features").GetArrayLength();
    }

    private static async Task<int> FilasAuditadasAsync(EscenarioAnomalias escenario, Guid actor)
    {
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var registro = await scope.ServiceProvider.GetRequiredService<AuditoriaDbContext>().Registros
            .SingleAsync(r => r.ActorId == actor && r.Accion == (short)AccionAuditada.DatasetExportado, EscenarioAnomalias.Ct);
        using var contexto = JsonDocument.Parse(registro.Contexto);
        return contexto.RootElement.GetProperty("filas").GetInt32();
    }
}
