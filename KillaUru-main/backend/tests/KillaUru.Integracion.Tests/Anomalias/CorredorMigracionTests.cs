using KillaUru.Anomalias.Application.Procesamiento;
using KillaUru.Anomalias.Domain;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>Comprueba V0005 y la carga real; el corredor sintético de la fábrica no participa.</summary>
[Collection("Anomalias")]
public sealed class CorredorMigracionTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Migracion_conserva_vertices_y_cargador_habilita_disponibilidad()
    {
        using var api = new FabricaApi(bd);
        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        var fila = await db.Corredores.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(6689, fila.Traza.NumPoints);
        Assert.Equal(4326, fila.Traza.SRID);
        Assert.Equal(-76.1597883, fila.Traza.StartPoint.X, 7);
        Assert.Equal(-74.2112089, fila.Traza.EndPoint.X, 7);
        var estado = new EstadoCorredor();
        var salud = new CorredorHealthCheck(estado);
        Assert.Equal(HealthStatus.Unhealthy, (await salud.CheckHealthAsync(new(), TestContext.Current.CancellationToken)).Status);
        var cargador = new CargadorCorredor(api.Services.GetRequiredService<IServiceScopeFactory>(), estado, Options.Create(new AnomaliasOptions()));
        await cargador.StartAsync(TestContext.Current.CancellationToken);
        Assert.Equal(335.0729634613575, estado.Actual!.LongitudKm, 8);
        Assert.Equal(HealthStatus.Healthy, (await salud.CheckHealthAsync(new(), TestContext.Current.CancellationToken)).Status);
    }
}
