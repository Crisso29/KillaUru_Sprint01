using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Contracts.Lecturas;
using KillaUru.Dashboard.Application;
using KillaUru.Dashboard.Application.Puertos;
using KillaUru.Dashboard.Domain;
using KillaUru.Identidad.Contracts;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace KillaUru.Dashboard.Tests.Application;

/// <summary>SC-002 / SC-003: llenado de huecos dentro de la ventana recuperable y purga.</summary>
public sealed class CalcularIndicadoresUseCaseTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 23, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Hoy = new(2026, 10, 23); // Lima -5 → 05:00 local = still Oct 23

    private static CalcularIndicadoresUseCase Crear(
        IIndicadoresRepositorio repositorio,
        IAnomaliaLectura? anomalias = null,
        IIdentidadConsultas? identidad = null,
        IMetricas? metricas = null,
        int diasRecuperables = 2)
    {
        var opts = Options.Create(new DashboardOptions { DiasRecuperables = diasRecuperables, DesfaseHorarioHoras = -5 });
        var reloj = new FakeTimeProvider(Ahora);
        return new CalcularIndicadoresUseCase(
            repositorio,
            anomalias ?? Substitute.For<IAnomaliaLectura>(),
            identidad ?? Substitute.For<IIdentidadConsultas>(),
            opts,
            new CalculadoUtcHolder(),
            metricas ?? Substitute.For<IMetricas>(),
            reloj);
    }

    [Fact]
    public async Task SC_002_calcula_los_dias_faltantes_dentro_de_la_ventana()
    {
        // Arrange
        var repositorio = Substitute.For<IIndicadoresRepositorio>();
        var anomalias   = Substitute.For<IAnomaliaLectura>();
        var identidad   = Substitute.For<IIdentidadConsultas>();

        // Solo D-2 está guardado; D-1 falta
        repositorio.DiasCompletosAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<DateOnly> { Hoy.AddDays(-2) });
        anomalias.ResumenDelDiaAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(ResumenVacio(Hoy.AddDays(-1)));
        identidad.ContarConductoresActivosAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(42);

        var uc = Crear(repositorio, anomalias, identidad);

        // Act
        await uc.EjecutarAsync(CancellationToken.None);

        // Assert — solo se calcula D-1 (D-2 ya estaba)
        await anomalias.Received(1).ResumenDelDiaAsync(Hoy.AddDays(-1), Arg.Any<CancellationToken>());
        await repositorio.Received(1).GuardarDiaAsync(Arg.Any<DiaIndicadores>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SC_003_dias_fuera_de_ventana_no_se_calculan()
    {
        // Arrange
        var repositorio = Substitute.For<IIndicadoresRepositorio>();
        repositorio.DiasCompletosAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<DateOnly>());

        var anomalias = Substitute.For<IAnomaliaLectura>();
        anomalias.ResumenDelDiaAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(c => ResumenVacio((DateOnly)c[0]));

        var identidad = Substitute.For<IIdentidadConsultas>();
        identidad.ContarConductoresActivosAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(0);

        var uc = Crear(repositorio, anomalias, identidad, diasRecuperables: 1);

        // Act
        await uc.EjecutarAsync(CancellationToken.None);

        // Assert — con recuperables=1 solo se intenta D-1, no D-2 ni antes
        await repositorio.DidNotReceive().GuardarDiaAsync(
            Arg.Is<DiaIndicadores>(d => d.Dia <= Hoy.AddDays(-2)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Registra_tarea_correcta_al_finalizar()
    {
        // Arrange
        var repositorio = Substitute.For<IIndicadoresRepositorio>();
        repositorio.DiasCompletosAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<DateOnly> { Hoy.AddDays(-1), Hoy.AddDays(-2) }); // todo guardado
        var metricas = Substitute.For<IMetricas>();
        var uc = Crear(repositorio, metricas: metricas);

        // Act
        await uc.EjecutarAsync(CancellationToken.None);

        // Assert
        metricas.Received(1).RegistrarTareaCorrecta("indicadores");
    }

    private static ResumenDia ResumenVacio(DateOnly dia) =>
        new(dia, 0L, 0L, 0, 0, 0L, 0L, 0L, 0L);
}
