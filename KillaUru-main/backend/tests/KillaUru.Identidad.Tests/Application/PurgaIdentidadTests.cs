using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Tareas;
using KillaUru.Identidad.Infrastructure.Tareas;
using KillaUru.Shared.Metricas;
using KillaUru.Shared.Tareas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace KillaUru.Identidad.Tests.Application;

/// <summary>
/// Purga diaria de sesiones y enlaces viejos: corre al arrancar y a las 03:45 de Lima, y solo
/// reporta éxito si terminó (la alerta de Grafana vigila esa métrica). Spec 003 · FR-040.
/// </summary>
public sealed class PurgaIdentidadTests
{
    private readonly IPurgaIdentidadRepositorio _repositorio = Substitute.For<IPurgaIdentidadRepositorio>();
    private readonly IMetricas _metricas = Substitute.For<IMetricas>();
    private readonly IOptions<IdentidadOptions> _opciones = Options.Create(new IdentidadOptions());

    /// <summary>Al arrancar no espera a las 03:45: purga enseguida con 30 días de retención.</summary>
    [Fact]
    public async Task Arranque_purga_de_inmediato_y_reporta_exito()
    {
        // Arrange
        var reloj = new FakeTimeProvider();
        var terminada = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _metricas.When(m => m.RegistrarTareaCorrecta("purga-identidad")).Do(_ => terminada.TrySetResult());
        var servicios = new ServiceCollection().AddScoped(_ => new PurgarIdentidadUseCase(_repositorio, _metricas, _opciones, reloj));
        await using var proveedor = servicios.BuildServiceProvider();
        using var tarea = new PurgaIdentidadTarea(proveedor.GetRequiredService<IServiceScopeFactory>(), reloj, _opciones,
            NullLogger<PurgaIdentidadTarea>.Instance);

        // Act
        await tarea.StartAsync(TestContext.Current.CancellationToken);
        await terminada.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await tarea.StopAsync(TestContext.Current.CancellationToken);

        // Assert
        await _repositorio.Received(1).PurgarAsync(reloj.GetUtcNow(), 30, 2, Arg.Any<CancellationToken>());
    }

    /// <summary>Si la purga falla, no se registra éxito: así la alerta de tarea atrasada salta.</summary>
    [Fact]
    public async Task Fallo_no_reporta_exito()
    {
        // Arrange
        _repositorio.PurgarAsync(Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Fallo simulado")));
        var caso = new PurgarIdentidadUseCase(_repositorio, _metricas, _opciones, new FakeTimeProvider());

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => caso.EjecutarAsync(TestContext.Current.CancellationToken));
        _metricas.DidNotReceive().RegistrarTareaCorrecta(Arg.Any<string>());
    }

    /// <summary>
    /// El horario es por reloj (03:45 Lima = 08:45 UTC), no «cada 24 h desde el arranque»: un
    /// reinicio no corre la hora de la purga.
    /// </summary>
    [Theory]
    [InlineData(44, 59, 1)]
    [InlineData(45, 0, 86400)]
    public void Proxima_purga_es_a_las_0345_de_Lima(int minutoUtc, int segundoUtc, int segundosHastaLaProxima)
    {
        // Arrange
        var ahora = new DateTimeOffset(2026, 10, 23, 8, minutoUtc, segundoUtc, TimeSpan.Zero);
        var hora = new IdentidadOptions().HoraPurgaIdentidad;

        // Act
        var proxima = ProgramaReloj.ProximaDiaria(ahora, hora);

        // Assert
        Assert.Equal(ahora.AddSeconds(segundosHastaLaProxima), proxima);
    }
}
