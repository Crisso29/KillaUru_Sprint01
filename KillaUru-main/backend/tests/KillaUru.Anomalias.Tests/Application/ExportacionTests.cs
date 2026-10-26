using System.Text;
using System.Text.Json;
using KillaUru.Anomalias.Application.Exportacion;
using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Infrastructure.Exportacion;
using KillaUru.Auditoria.Contracts;
using NSubstitute;
namespace KillaUru.Anomalias.Tests.Application;
/// <summary>Formato incremental y auditoría obligatoria antes de exponer datos.</summary>
public sealed class ExportacionTests
{
    [Fact]
    public async Task Csv_escapa_comillas_y_geojson_comparte_columnas_sin_seudonimo()
    {
        var ct = TestContext.Current.CancellationToken;
        using var csv = new MemoryStream(); await new EscritorCsv().EscribirAsync(csv, Filas(), ct);
        var texto = Encoding.UTF8.GetString(csv.ToArray());
        Assert.Contains("\"Ba\"\"che,prueba\"", texto, StringComparison.Ordinal);
        Assert.DoesNotContain("seudonimo", texto, StringComparison.OrdinalIgnoreCase);
        using var json = new MemoryStream(); await new EscritorGeoJson().EscribirAsync(json, Filas(), ct);
        using var doc = JsonDocument.Parse(json.ToArray());
        var feature = doc.RootElement.GetProperty("features")[0];
        Assert.Equal("Point", feature.GetProperty("geometry").GetProperty("type").GetString());
        Assert.Equal(11, feature.GetProperty("properties").EnumerateObject().Count());
        Assert.True(feature.GetProperty("properties").TryGetProperty("primer_reporte", out _));
    }
    [Fact]
    public async Task Fallo_de_auditoria_no_escribe_ni_lee_filas()
    {
        var repo = Substitute.For<IExportacionRepository>(); var auditoria = Substitute.For<IAuditoriaService>();
        var escritor = Substitute.For<IEscritorExportacion>(); escritor.Formato.Returns("CSV");
        repo.AbrirInstantaneaAsync(Arg.Any<CancellationToken>()).Returns(Substitute.For<IAsyncDisposable>());
        auditoria.RegistrarLecturaAsync(AccionAuditada.DatasetExportado, Arg.Any<ActorAuditoria>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>()).Returns(Task.FromException(new InvalidOperationException("fallo")));
        using var destino = new MemoryStream();
        await Assert.ThrowsAsync<InvalidOperationException>(() => new ExportarDatasetUseCase(repo, auditoria, [escritor])
            .EjecutarAsync(new("CSV"), new(Guid.NewGuid(), "Administrador"), destino, TestContext.Current.CancellationToken));
        Assert.Equal(0, destino.Length);
        Assert.DoesNotContain(escritor.ReceivedCalls(), c => c.GetMethodInfo().Name == "EscribirAsync");
    }
    private static async IAsyncEnumerable<FilaExportacion> Filas()
    {
        await Task.CompletedTask;
        yield return new(Guid.Empty, "Ba\"che,prueba", 2, "Activa", "Episodica", 2.5f, -13, -75, 1, DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch);
    }
}

