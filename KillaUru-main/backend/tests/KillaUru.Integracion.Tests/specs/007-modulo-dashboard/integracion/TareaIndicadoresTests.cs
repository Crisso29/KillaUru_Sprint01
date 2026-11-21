using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Contracts.Lecturas;
using KillaUru.Dashboard.Application.Puertos;
using KillaUru.Identidad.Contracts;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Metricas;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace KillaUru.Integracion.Tests.Dashboard;

/// <summary>T015: TareaIndicadoresDiarios llena huecos al arrancar (SC-002).</summary>
public sealed class TareaIndicadoresTests(BaseDatosPrueba bd)
{
    private static readonly DateOnly Hoy = new(2026, 10, 21); // reloj fijo en EscenarioDashboard → 10:00 UTC = 05:00 Lima = Oct 21

    [Fact]
    public async Task Tarea_calcula_dias_faltantes_al_arrancar()
    {
        // Arrange
        var anomalias = Substitute.For<IAnomaliaLectura>();
        var identidad = Substitute.For<IIdentidadConsultas>();
        var metricas  = Substitute.For<IMetricas>();

        anomalias.ResumenDelDiaAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(c => ResumenVacio((DateOnly)c[0]));
        identidad.ContarConductoresActivosAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(42);

        using var escenario = new EscenarioDashboard(bd);
        using var api = new FabricaApi(bd).WithWebHostBuilder(b => b.ConfigureTestServices(s =>
        {
            s.AddSingleton<TimeProvider>(escenario.Reloj);
            s.AddSingleton(anomalias);
            s.AddSingleton(identidad);
            s.AddSingleton(metricas);
        }));

        // Act — crear la API hace que el host inicie las tareas en background
        _ = api.CreateClient();
        await Task.Delay(500, EscenarioDashboard.Ct);

        // Assert — la tarea llamó a la métrica de éxito
        metricas.Received().RegistrarTareaCorrecta("indicadores");
    }

    [Fact]
    public async Task Dias_ya_guardados_no_se_recalculan()
    {
        // Arrange
        using var escenario = new EscenarioDashboard(bd);
        var diaGuardado = Hoy.AddDays(-1);
        await escenario.GuardarDiaAsync(EscenarioDashboard.DiaSintetico(diaGuardado));

        var anomalias = Substitute.For<IAnomaliaLectura>();
        anomalias.ResumenDelDiaAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(c => ResumenVacio((DateOnly)c[0]));
        escenario.Identidad.ContarConductoresActivosAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(42);

        using var api = new FabricaApi(bd).WithWebHostBuilder(b => b.ConfigureTestServices(s =>
        {
            s.AddSingleton<TimeProvider>(escenario.Reloj);
            s.AddSingleton(anomalias);
            s.AddSingleton(escenario.Identidad);
            s.AddSingleton(escenario.Metricas);
        }));

        // Act
        _ = api.CreateClient();
        await Task.Delay(500, EscenarioDashboard.Ct);

        // Assert — el día ya guardado no vuelve a llamarse
        await anomalias.DidNotReceive().ResumenDelDiaAsync(diaGuardado, Arg.Any<CancellationToken>());
    }

    private static ResumenDia ResumenVacio(DateOnly dia) => new(dia, 0L, 0L, 0, 0, 0L, 0L, 0L, 0L);
}
