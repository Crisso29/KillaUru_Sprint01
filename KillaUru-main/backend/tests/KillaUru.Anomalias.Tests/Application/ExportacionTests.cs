using System.Text;
using System.Text.Json;
using KillaUru.Anomalias.Application.Exportacion;
using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Infrastructure.Exportacion;
using KillaUru.Auditoria.Contracts;
using NSubstitute;

namespace KillaUru.Anomalias.Tests.Application;

/// <summary>
/// Exportación del dataset: CSV y GeoJSON con las mismas 11 columnas y sin seudónimos; la auditoría
/// se registra antes de leer una sola fila. Spec 004 · FR-028.
/// </summary>
public sealed class ExportacionTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>Una fila con comillas y coma en el texto, para probar el escape.</summary>
    private static async IAsyncEnumerable<FilaExportacion> UnaFila()
    {
        await Task.CompletedTask;
        yield return new(Guid.Empty, "Ba\"che,prueba", 2, "Activa", "Episodica", 2.5f, -13, -75, 1, DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch);
    }

    /// <summary>El CSV escapa comillas y comas según RFC 4180 y nunca incluye el seudónimo.</summary>
    [Fact]
    public async Task Csv_escapa_comillas_y_no_incluye_seudonimo()
    {
        // Arrange
        using var destino = new MemoryStream();

        // Act
        await new EscritorCsv().EscribirAsync(destino, UnaFila(), Ct);

        // Assert
        var texto = Encoding.UTF8.GetString(destino.ToArray());
        Assert.Contains("\"Ba\"\"che,prueba\"", texto, StringComparison.Ordinal);
        Assert.DoesNotContain("seudonimo", texto, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>El GeoJSON es un Point por anomalía con las mismas 11 columnas que el CSV.</summary>
    [Fact]
    public async Task GeoJson_comparte_las_columnas_del_csv()
    {
        // Arrange
        using var destino = new MemoryStream();

        // Act
        await new EscritorGeoJson().EscribirAsync(destino, UnaFila(), Ct);

        // Assert
        using var documento = JsonDocument.Parse(destino.ToArray());
        var feature = documento.RootElement.GetProperty("features")[0];
        Assert.Equal("Point", feature.GetProperty("geometry").GetProperty("type").GetString());
        Assert.Equal(11, feature.GetProperty("properties").EnumerateObject().Count());
        Assert.True(feature.GetProperty("properties").TryGetProperty("primer_reporte", out _));
    }

    /// <summary>Sin rastro de auditoría no sale ningún dato: ni se escribe ni se lee una fila (FR-028).</summary>
    [Fact]
    public async Task Fallo_de_auditoria_no_escribe_ni_lee_filas()
    {
        // Arrange
        var repositorio = Substitute.For<IExportacionRepository>();
        var auditoria = Substitute.For<IAuditoriaService>();
        var escritor = Substitute.For<IEscritorExportacion>();
        escritor.Formato.Returns("CSV");
        repositorio.AbrirInstantaneaAsync(Arg.Any<CancellationToken>()).Returns(Substitute.For<IAsyncDisposable>());
        auditoria.RegistrarLecturaAsync(AccionAuditada.DatasetExportado, Arg.Any<ActorAuditoria>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>()).Returns(Task.FromException(new InvalidOperationException("fallo")));
        var caso = new ExportarDatasetUseCase(repositorio, auditoria, [escritor]);
        using var destino = new MemoryStream();

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => caso.EjecutarAsync(new("CSV"), new(Guid.NewGuid(), "Administrador"), destino, Ct));
        Assert.Equal(0, destino.Length);
        Assert.DoesNotContain(escritor.ReceivedCalls(), c => c.GetMethodInfo().Name == nameof(IEscritorExportacion.EscribirAsync));
    }
}
