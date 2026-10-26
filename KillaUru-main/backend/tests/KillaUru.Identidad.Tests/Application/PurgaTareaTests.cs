using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Tareas;
using KillaUru.Identidad.Infrastructure.Tareas;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
namespace KillaUru.Identidad.Tests.Application;

/// <summary>La tarea arranca sin esperar al horario y solo anuncia una purga terminada.</summary>
public sealed class PurgaTareaTests
{
    [Fact]
    public async Task Arranque_ejecuta_purga_y_registra_exito()
    {
        var repo = Substitute.For<IPurgaIdentidadRepositorio>();
        var metricas = Substitute.For<IMetricas>();
        var terminada = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        metricas.When(x => x.RegistrarTareaCorrecta("purga-identidad")).Do(_ => terminada.TrySetResult());
        var reloj = new FakeTimeProvider();
        var opciones = Options.Create(new IdentidadOptions());
        var servicios = new ServiceCollection().AddScoped(_ => new PurgarIdentidadUseCase(repo, metricas, opciones, reloj));
        await using var provider = servicios.BuildServiceProvider();
        using var tarea = new PurgaIdentidadTarea(provider.GetRequiredService<IServiceScopeFactory>(), reloj, opciones, NullLogger<PurgaIdentidadTarea>.Instance);
        await tarea.StartAsync(TestContext.Current.CancellationToken);
        await terminada.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await tarea.StopAsync(TestContext.Current.CancellationToken);
        await repo.Received(1).PurgarAsync(reloj.GetUtcNow(), 30, 2, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Fallo_de_purga_no_registra_exito()
    {
        var repo = Substitute.For<IPurgaIdentidadRepositorio>();
        var metricas = Substitute.For<IMetricas>();
        repo.PurgarAsync(Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Fallo simulado")));
        var caso = new PurgarIdentidadUseCase(repo, metricas, Options.Create(new IdentidadOptions()), new FakeTimeProvider());
        await Assert.ThrowsAsync<InvalidOperationException>(() => caso.EjecutarAsync(TestContext.Current.CancellationToken));
        metricas.DidNotReceive().RegistrarTareaCorrecta(Arg.Any<string>());
    }
}
