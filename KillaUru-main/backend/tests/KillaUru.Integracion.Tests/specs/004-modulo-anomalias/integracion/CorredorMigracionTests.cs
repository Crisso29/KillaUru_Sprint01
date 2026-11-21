using KillaUru.Anomalias.Application.Procesamiento;
using KillaUru.Anomalias.Domain;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// La migración V0005 guarda la PE-28A completa y el cargador real la sube a memoria al arrancar;
/// aquí no participa el corredor sintético de la fábrica. Spec 004 · FR-003, FR-033.
/// </summary>
[Collection("Anomalias")]
public sealed class CorredorMigracionTests(BaseDatosPrueba bd)
{
    /// <summary>La polilínea migrada conserva sus 6 689 vértices en WGS84 y sus extremos exactos.</summary>
    [Fact]
    public async Task Migracion_conserva_los_vertices_de_la_PE28A()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        await using var scope = api.Services.CreateAsyncScope();

        // Act
        var fila = await scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>().Corredores.AsNoTracking().SingleAsync(EscenarioAnomalias.Ct);

        // Assert
        Assert.Equal(6689, fila.Traza.NumPoints);
        Assert.Equal(4326, fila.Traza.SRID);
        Assert.Equal(-76.1597883, fila.Traza.StartPoint.X, 7);
        Assert.Equal(-74.2112089, fila.Traza.EndPoint.X, 7);
    }

    /// <summary>El chequeo de salud está en rojo hasta que el cargador sube los 335 km a memoria.</summary>
    [Fact]
    public async Task Cargador_habilita_la_disponibilidad()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var corredor = new EstadoCorredor();
        var salud = new CorredorHealthCheck(corredor);
        var antes = (await salud.CheckHealthAsync(new(), EscenarioAnomalias.Ct)).Status;
        var cargador = new CargadorCorredor(api.Services.GetRequiredService<IServiceScopeFactory>(), corredor, Options.Create(new AnomaliasOptions()));

        // Act
        await cargador.StartAsync(EscenarioAnomalias.Ct);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, antes);
        Assert.Equal(335.0729634613575, corredor.Actual!.LongitudKm, 8);
        Assert.Equal(HealthStatus.Healthy, (await salud.CheckHealthAsync(new(), EscenarioAnomalias.Ct)).Status);
    }
}
