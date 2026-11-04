using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Contracts.Lecturas;
using KillaUru.Dashboard.Application;
using KillaUru.Dashboard.Application.Puertos;
using KillaUru.Dashboard.Infrastructure;
using KillaUru.Identidad.Contracts;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace KillaUru.Dashboard.Tests.Infrastructure;

/// <summary>SC-001 / SC-002: la tarea arranca inmediatamente y espera hasta la siguiente hora local.</summary>
public sealed class TareaIndicadoresDiariosTests
{
    [Fact]
    public async Task Ejecuta_el_use_case_al_arrancar()
    {
        // Arrange
        var ahora  = new DateTimeOffset(2026, 10, 21, 3, 0, 0, TimeSpan.Zero);
        var reloj  = new FakeTimeProvider(ahora);
        var repositorio = Substitute.For<IIndicadoresRepositorio>();
        repositorio.DiasCompletosAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<DateOnly> { new DateOnly(2026, 10, 19), new DateOnly(2026, 10, 20) });
        var metricas = Substitute.For<IMetricas>();

        var anomalias = Substitute.For<IAnomaliaLectura>();
        anomalias.ResumenDelDiaAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(c => new ResumenDia((DateOnly)c[0], 0L, 0L, 0, 0, 0L, 0L, 0L, 0L));
        var identidad = Substitute.For<IIdentidadConsultas>();
        identidad.ContarConductoresActivosAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(0);

        var opts = Options.Create(new DashboardOptions { DesfaseHorarioHoras = -5, HoraCalculoLocal = new TimeOnly(0, 30) });
        var services = new ServiceCollection()
            .AddSingleton<TimeProvider>(reloj)
            .AddSingleton<IOptions<DashboardOptions>>(opts)
            .AddSingleton<CalculadoUtcHolder>()
            .AddSingleton<IIndicadoresRepositorio>(repositorio)
            .AddSingleton(anomalias)
            .AddSingleton(identidad)
            .AddSingleton(metricas)
            .AddScoped<CalcularIndicadoresUseCase>()
            .BuildServiceProvider();

        var scopes = services.GetRequiredService<IServiceScopeFactory>();
        var log    = NullLogger<TareaIndicadoresDiarios>.Instance;
        var tarea  = new TareaIndicadoresDiarios(scopes, reloj, opts, log);

        // Act
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        try { await tarea.StartAsync(cts.Token); await Task.Delay(200, cts.Token); }
        catch (OperationCanceledException) { /* ok */ }

        // Assert — el use case corrió al arrancar
        metricas.Received(1).RegistrarTareaCorrecta("indicadores");
    }
}
