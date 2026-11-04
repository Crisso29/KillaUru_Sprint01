using KillaUru.Anomalias.Contracts;
using KillaUru.Ingesta.Application.Procesamiento;
using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Ingesta.Domain;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace KillaUru.Ingesta.Tests.Application;

/// <summary>
/// ProcesarLoteUseCase carga los eventos pendientes de un lote, los entrega a Anomalías en orden
/// de detectado_utc con el ContextoReporte correcto, marca el estado resultante y registra el
/// retraso de cola (FR-012…FR-014, FR-026; R-005, R-015). Spec 005 · T019.
/// </summary>
public sealed class ProcesarLoteUseCaseTests
{
    private static readonly Guid LoteId = Guid.NewGuid();
    private static readonly Guid S = Guid.NewGuid();
    private static readonly DateTimeOffset Ahora = new(2026, 10, 20, 14, 32, 05, TimeSpan.Zero);

    private static EventoPendiente Pendiente(DateTimeOffset detectado, bool conSesion = false) =>
        new(new EventoReportado(Guid.NewGuid(), detectado, -13.0, -74.0, 5.0, "Bache", 4, 0.91, "1.0", S),
            conSesion, Ahora.AddMinutes(-2));

    private static (ProcesarLoteUseCase uc, IPendientesRepositorio repo,
        IAnomaliasService anomalias, IMetricas metricas, FakeTimeProvider reloj)
    Crear()
    {
        var repo = Substitute.For<IPendientesRepositorio>();
        var anomalias = Substitute.For<IAnomaliasService>();
        var metricas = Substitute.For<IMetricas>();
        var reloj = new FakeTimeProvider();
        reloj.SetUtcNow(Ahora);
        anomalias.ProcesarReporteAsync(Arg.Any<EventoReportado>(), Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>())
                 .Returns(new ResultadoProceso());
        var armador = new ArmadorContexto(repo);
        var uc = new ProcesarLoteUseCase(repo, anomalias, armador, metricas, reloj);
        return (uc, repo, anomalias, metricas, reloj);
    }

    /// <summary>Los eventos se entregan a Anomalías en orden ascendente de detectado_utc (FR-012, R-005).</summary>
    [Fact]
    public async Task Eventos_se_entregan_en_orden_de_deteccion()
    {
        // Arrange
        var t1 = Ahora.AddMinutes(-10);
        var t2 = Ahora.AddMinutes(-5);
        var p1 = Pendiente(t1);
        var p2 = Pendiente(t2);
        var (uc, repo, anomalias, _, _) = Crear();
        repo.RecibidosDelLoteAsync(LoteId, Arg.Any<CancellationToken>())
            .Returns([p2, p1]);  // llegan desordenados
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var orden = new List<Guid>();
        await anomalias.ProcesarReporteAsync(
            Arg.Do<EventoReportado>(e => orden.Add(e.EventoId)),
            Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>());

        // Act
        await uc.EjecutarAsync(LoteId, CancellationToken.None);

        // Assert
        Assert.Equal(p1.Evento.EventoId, orden[0]);
        Assert.Equal(p2.Evento.EventoId, orden[1]);
    }

    /// <summary>Resultado Procesado marca la fila como Procesado (FR-014).</summary>
    [Fact]
    public async Task Resultado_procesado_marca_fila_procesado()
    {
        // Arrange
        var p = Pendiente(Ahora.AddMinutes(-5));
        var (uc, repo, anomalias, _, _) = Crear();
        repo.RecibidosDelLoteAsync(LoteId, Arg.Any<CancellationToken>()).Returns([p]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(1);
        anomalias.ProcesarReporteAsync(Arg.Any<EventoReportado>(), Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>())
                 .Returns(new ResultadoProceso());

        // Act
        await uc.EjecutarAsync(LoteId, CancellationToken.None);

        // Assert
        await repo.Received(1).MarcarAsync(p.Evento.EventoId, EstadoRecepcion.Procesado, null, Arg.Any<CancellationToken>());
    }

    /// <summary>Resultado YaProcesado también marca la fila como Procesado (FR-014).</summary>
    [Fact]
    public async Task Resultado_ya_procesado_marca_fila_procesado()
    {
        // Arrange
        var p = Pendiente(Ahora.AddMinutes(-5));
        var (uc, repo, anomalias, _, _) = Crear();
        repo.RecibidosDelLoteAsync(LoteId, Arg.Any<CancellationToken>()).Returns([p]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(1);
        anomalias.ProcesarReporteAsync(Arg.Any<EventoReportado>(), Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>())
                 .Returns(new ResultadoProceso(YaProcesado: true));

        // Act
        await uc.EjecutarAsync(LoteId, CancellationToken.None);

        // Assert
        await repo.Received(1).MarcarAsync(p.Evento.EventoId, EstadoRecepcion.Procesado, null, Arg.Any<CancellationToken>());
    }

    /// <summary>Resultado Descartado marca la fila como Descartado con su motivo (FR-014).</summary>
    [Fact]
    public async Task Resultado_descartado_marca_fila_descartado_con_motivo()
    {
        // Arrange
        var p = Pendiente(Ahora.AddMinutes(-5));
        var (uc, repo, anomalias, _, _) = Crear();
        repo.RecibidosDelLoteAsync(LoteId, Arg.Any<CancellationToken>()).Returns([p]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(1);
        anomalias.ProcesarReporteAsync(Arg.Any<EventoReportado>(), Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>())
                 .Returns(new ResultadoProceso(Motivo: MotivoDescarte.FueraDelCorredor));

        // Act
        await uc.EjecutarAsync(LoteId, CancellationToken.None);

        // Assert
        await repo.Received(1).MarcarAsync(p.Evento.EventoId, EstadoRecepcion.Descartado,
            (short)MotivoDescarte.FueraDelCorredor, Arg.Any<CancellationToken>());
    }

    /// <summary>Una excepción en ProcesarReporteAsync deja la fila Recibido y sigue con el siguiente (FR-014, R-015).</summary>
    [Fact]
    public async Task Excepcion_en_procesar_deja_fila_recibido_y_continua()
    {
        // Arrange
        var p1 = Pendiente(Ahora.AddMinutes(-10));
        var p2 = Pendiente(Ahora.AddMinutes(-5));
        var (uc, repo, anomalias, _, _) = Crear();
        repo.RecibidosDelLoteAsync(LoteId, Arg.Any<CancellationToken>()).Returns([p1, p2]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(1);
        anomalias.ProcesarReporteAsync(
            Arg.Is<EventoReportado>(e => e.EventoId == p1.Evento.EventoId),
            Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("fallo simulado"));
        anomalias.ProcesarReporteAsync(
            Arg.Is<EventoReportado>(e => e.EventoId == p2.Evento.EventoId),
            Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>())
            .Returns(new ResultadoProceso());

        // Act
        await uc.EjecutarAsync(LoteId, CancellationToken.None);

        // Assert
        await repo.DidNotReceive().MarcarAsync(p1.Evento.EventoId, Arg.Any<EstadoRecepcion>(),
            Arg.Any<short?>(), Arg.Any<CancellationToken>());
        await repo.Received(1).MarcarAsync(p2.Evento.EventoId, EstadoRecepcion.Procesado,
            null, Arg.Any<CancellationToken>());
    }

    /// <summary>Al marcar una fila, se registra el retraso de cola (FR-026, D11).</summary>
    [Fact]
    public async Task Al_marcar_se_registra_el_retraso_de_cola()
    {
        // Arrange
        var recibido = Ahora.AddSeconds(-10);
        var p = new EventoPendiente(
            new EventoReportado(Guid.NewGuid(), recibido, -13.0, -74.0, 5.0, "Bache", 4, 0.91, "1.0", S),
            false, recibido);
        var (uc, repo, anomalias, metricas, reloj) = Crear();
        repo.RecibidosDelLoteAsync(LoteId, Arg.Any<CancellationToken>()).Returns([p]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(1);
        anomalias.ProcesarReporteAsync(Arg.Any<EventoReportado>(), Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>())
                 .Returns(new ResultadoProceso());

        // Act
        await uc.EjecutarAsync(LoteId, CancellationToken.None);

        // Assert
        metricas.Received(1).RegistrarRetrasoCola(Arg.Is<TimeSpan>(t => t.TotalSeconds > 0));
    }

    /// <summary>El contexto incluye ConSesion, EventosEnLaHora y RecibidoUtc (FR-013, RN-15a).</summary>
    [Fact]
    public async Task Contexto_incluye_conSesion_eventosEnHora_y_recibidoUtc()
    {
        // Arrange
        var recibido = Ahora.AddMinutes(-2);
        var p = new EventoPendiente(
            new EventoReportado(Guid.NewGuid(), recibido, -13.0, -74.0, 5.0, "Bache", 4, 0.91, "1.0", S),
            true, recibido);
        var (uc, repo, anomalias, _, _) = Crear();
        repo.RecibidosDelLoteAsync(LoteId, Arg.Any<CancellationToken>()).Returns([p]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(5);

        ContextoReporte? capturado = null;
        anomalias.ProcesarReporteAsync(
            Arg.Any<EventoReportado>(),
            Arg.Do<ContextoReporte>(c => capturado = c),
            Arg.Any<CancellationToken>())
            .Returns(new ResultadoProceso());

        // Act
        await uc.EjecutarAsync(LoteId, CancellationToken.None);

        // Assert
        Assert.NotNull(capturado);
        Assert.True(capturado!.ConSesion);
        Assert.Equal(5, capturado.EventosEnLaHora);
        Assert.Equal(recibido, capturado.RecibidoUtc);
    }
}
