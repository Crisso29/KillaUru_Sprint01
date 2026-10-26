using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Application.Tareas;
using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Domain;
using KillaUru.Anomalias.Infrastructure.Tareas;
using KillaUru.Shared.Eventos;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace KillaUru.Anomalias.Tests.Application;

/// <summary>
/// Cada tarea corre al arrancar y luego a su hora de reloj, no «cada N horas desde el arranque»:
/// un reinicio no corre los horarios. Se arranca a las 03:29 de Lima (08:29 UTC). Spec 004 · FR-018, FR-021, FR-022, FR-024.
/// </summary>
public sealed class TareasProgramadasTests
{
    private static readonly DateTimeOffset ArranqueUtc = new(2026, 10, 25, 8, 29, 0, TimeSpan.Zero);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>
    /// Caducidad en el minuto :05 (faltan 36 min), purga de seudónimos en el :10 (41 min) y purga
    /// histórica a las 03:30 de Lima (1 min). Un segundo antes no corre; justo a la hora, sí.
    /// </summary>
    [Theory]
    [InlineData(0, 36, "caducidad")]
    [InlineData(1, 41, "purga-seudonimos")]
    [InlineData(2, 1, "purga-historica")]
    public async Task Arranca_y_repite_a_su_hora_de_reloj(int tipoTarea, int minutosHastaLaSiguiente, string nombre)
    {
        // Arrange
        var reloj = new FakeTimeProvider(ArranqueUtc);
        var metricas = Substitute.For<IMetricas>();
        var ejecuciones = 0;
        metricas.When(m => m.RegistrarTareaCorrecta(nombre)).Do(_ => Interlocked.Increment(ref ejecuciones));
        using var servicios = Servicios(reloj, metricas);
        using var tarea = Crear(tipoTarea, servicios.GetRequiredService<IServiceScopeFactory>(), reloj);

        // Act + Assert — una al arrancar, ninguna un segundo antes, otra justo a la hora
        await tarea.StartAsync(Ct);
        await EsperarAsync(() => Volatile.Read(ref ejecuciones) == 1, Ct);
        reloj.Advance(TimeSpan.FromMinutes(minutosHastaLaSiguiente) - TimeSpan.FromSeconds(1));
        await Task.Delay(30, Ct);
        Assert.Equal(1, Volatile.Read(ref ejecuciones));
        reloj.Advance(TimeSpan.FromSeconds(1));
        await EsperarAsync(() => Volatile.Read(ref ejecuciones) == 2, Ct);
        await tarea.StopAsync(Ct);
    }

    private static BackgroundService Crear(int tipoTarea, IServiceScopeFactory scopes, TimeProvider reloj) => tipoTarea switch
    {
        0 => new TareaCaducidad(scopes, reloj, NullLogger<TareaCaducidad>.Instance),
        1 => new TareaPurgaSeudonimos(scopes, reloj, NullLogger<TareaPurgaSeudonimos>.Instance),
        _ => new TareaPurgaHistorica(scopes, reloj, NullLogger<TareaPurgaHistorica>.Instance),
    };

    /// <summary>Casos de uso reales con repositorios sustituidos y una transacción que siempre confirma.</summary>
    private static ServiceProvider Servicios(TimeProvider reloj, IMetricas metricas)
    {
        var transaccion = Substitute.For<ITransaccionAnomalias>();
        transaccion.EjecutarAsync(Arg.Any<IReadOnlyCollection<long>>(), null, null, Arg.Any<Func<Task<int>>>(), Arg.Any<CancellationToken>())
            .Returns(llamada => llamada.Arg<Func<Task<int>>>()());
        var caducidad = Substitute.For<ICaducidadRepository>();
        caducidad.CaducarAsync(Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(Array.Empty<AnomaliaCambiada>());
        return new ServiceCollection()
            .AddSingleton(reloj).AddSingleton(metricas).AddSingleton(transaccion).AddSingleton(caducidad)
            .AddSingleton(Substitute.For<IPurgasRepository>()).AddSingleton(Substitute.For<IPublicadorEventos>())
            .AddSingleton(Options.Create(new AnomaliasOptions()))
            .AddScoped<CaducarAnomaliasUseCase>().AddScoped<PurgarSeudonimosUseCase>().AddScoped<PurgarHistoricoUseCase>()
            .BuildServiceProvider();
    }

    /// <summary>Espera hasta 2 s a que la tarea en segundo plano cumpla la condición.</summary>
    private static async Task EsperarAsync(Func<bool> condicion, CancellationToken ct)
    {
        for (var intento = 0; intento < 200 && !condicion(); intento++) await Task.Delay(10, ct);
        Assert.True(condicion());
        await Task.Delay(30, ct); // margen para detectar una ejecución de más
    }
}
