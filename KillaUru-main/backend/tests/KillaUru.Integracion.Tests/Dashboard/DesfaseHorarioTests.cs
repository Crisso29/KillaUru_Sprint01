using KillaUru.Anomalias.Domain;
using KillaUru.Dashboard.Application;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KillaUru.Integracion.Tests.Dashboard;

/// <summary>T025 / SC-012 (R-008): Dashboard y Anomalías deben usar el mismo DesfaseHorarioHoras.</summary>
public sealed class DesfaseHorarioTests(BaseDatosPrueba bd)
{
    [Fact]
    public void DesfaseHorario_de_Dashboard_igual_que_Anomalias()
    {
        // Arrange
        using var api = new FabricaApi(bd);

        // Act
        using var scope      = api.Services.CreateScope();
        var dashOpts         = scope.ServiceProvider.GetRequiredService<IOptions<DashboardOptions>>().Value;
        var anomaliasOpts    = scope.ServiceProvider.GetRequiredService<IOptions<AnomaliasOptions>>().Value;

        // Assert
        Assert.Equal(anomaliasOpts.DesfaseHorarioHoras, dashOpts.DesfaseHorarioHoras);
    }
}
