using KillaUru.Anomalias.Contracts;
using KillaUru.Ingesta.Application.Procesamiento;
using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Ingesta.Domain;
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
    [Fact]
    public async Task Eventos_se_entregan_en_orden_de_deteccion()
    {
        var t1 = ProcesarLoteTests_Setup.Ahora.AddMinutes(-10);
        var t2 = ProcesarLoteTests_Setup.Ahora.AddMinutes(-5);
        var p1 = ProcesarLoteTests_Setup.Pendiente(t1);
        var p2 = ProcesarLoteTests_Setup.Pendiente(t2);
        var (uc, repo, anomalias, _, _) = ProcesarLoteTests_Setup.Crear();
        repo.RecibidosDelLoteAsync(ProcesarLoteTests_Setup.LoteId, Arg.Any<CancellationToken>())
            .Returns([p2, p1]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(1);
        var orden = new List<Guid>();
        await anomalias.ProcesarReporteAsync(
            Arg.Do<EventoReportado>(e => orden.Add(e.EventoId)),
            Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>());
        await uc.EjecutarAsync(ProcesarLoteTests_Setup.LoteId, CancellationToken.None);
        Assert.Equal(p1.Evento.EventoId, orden[0]);
        Assert.Equal(p2.Evento.EventoId, orden[1]);
    }

    [Fact]
    public async Task Resultado_procesado_marca_fila_procesado()
    {
        var p = ProcesarLoteTests_Setup.Pendiente(ProcesarLoteTests_Setup.Ahora.AddMinutes(-5));
        var (uc, repo, anomalias, _, _) = ProcesarLoteTests_Setup.Crear();
        repo.RecibidosDelLoteAsync(ProcesarLoteTests_Setup.LoteId, Arg.Any<CancellationToken>()).Returns([p]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(1);
        anomalias.ProcesarReporteAsync(Arg.Any<EventoReportado>(), Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>())
                 .Returns(new ResultadoProceso());
        await uc.EjecutarAsync(ProcesarLoteTests_Setup.LoteId, CancellationToken.None);
        await repo.Received(1).MarcarAsync(p.Evento.EventoId, EstadoRecepcion.Procesado, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Resultado_ya_procesado_marca_fila_procesado()
    {
        var p = ProcesarLoteTests_Setup.Pendiente(ProcesarLoteTests_Setup.Ahora.AddMinutes(-5));
        var (uc, repo, anomalias, _, _) = ProcesarLoteTests_Setup.Crear();
        repo.RecibidosDelLoteAsync(ProcesarLoteTests_Setup.LoteId, Arg.Any<CancellationToken>()).Returns([p]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(1);
        anomalias.ProcesarReporteAsync(Arg.Any<EventoReportado>(), Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>())
                 .Returns(new ResultadoProceso(YaProcesado: true));
        await uc.EjecutarAsync(ProcesarLoteTests_Setup.LoteId, CancellationToken.None);
        await repo.Received(1).MarcarAsync(p.Evento.EventoId, EstadoRecepcion.Procesado, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Resultado_descartado_marca_fila_descartado_con_motivo()
    {
        var p = ProcesarLoteTests_Setup.Pendiente(ProcesarLoteTests_Setup.Ahora.AddMinutes(-5));
        var (uc, repo, anomalias, _, _) = ProcesarLoteTests_Setup.Crear();
        repo.RecibidosDelLoteAsync(ProcesarLoteTests_Setup.LoteId, Arg.Any<CancellationToken>()).Returns([p]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(1);
        anomalias.ProcesarReporteAsync(Arg.Any<EventoReportado>(), Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>())
                 .Returns(new ResultadoProceso(Motivo: MotivoDescarte.FueraDelCorredor));
        await uc.EjecutarAsync(ProcesarLoteTests_Setup.LoteId, CancellationToken.None);
        await repo.Received(1).MarcarAsync(p.Evento.EventoId, EstadoRecepcion.Descartado,
            (short)MotivoDescarte.FueraDelCorredor, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Excepcion_en_procesar_deja_fila_recibido_y_continua()
    {
        var p1 = ProcesarLoteTests_Setup.Pendiente(ProcesarLoteTests_Setup.Ahora.AddMinutes(-10));
        var p2 = ProcesarLoteTests_Setup.Pendiente(ProcesarLoteTests_Setup.Ahora.AddMinutes(-5));
        var (uc, repo, anomalias, _, _) = ProcesarLoteTests_Setup.Crear();
        repo.RecibidosDelLoteAsync(ProcesarLoteTests_Setup.LoteId, Arg.Any<CancellationToken>()).Returns([p1, p2]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(1);
        anomalias.ProcesarReporteAsync(
            Arg.Is<EventoReportado>(e => e.EventoId == p1.Evento.EventoId),
            Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("fallo simulado"));
        anomalias.ProcesarReporteAsync(
            Arg.Is<EventoReportado>(e => e.EventoId == p2.Evento.EventoId),
            Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>())
            .Returns(new ResultadoProceso());
        await uc.EjecutarAsync(ProcesarLoteTests_Setup.LoteId, CancellationToken.None);
        await repo.DidNotReceive().MarcarAsync(p1.Evento.EventoId, Arg.Any<EstadoRecepcion>(),
            Arg.Any<short?>(), Arg.Any<CancellationToken>());
        await repo.Received(1).MarcarAsync(p2.Evento.EventoId, EstadoRecepcion.Procesado,
            null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Al_marcar_se_registra_el_retraso_de_cola()
    {
        var recibido = ProcesarLoteTests_Setup.Ahora.AddSeconds(-10);
        var p = new EventoPendiente(
            new EventoReportado(Guid.NewGuid(), recibido, -13.0, -74.0, 5.0, "Bache", 4, 0.91, "1.0",
                ProcesarLoteTests_Setup.S),
            false, recibido);
        var (uc, repo, anomalias, metricas, _) = ProcesarLoteTests_Setup.Crear();
        repo.RecibidosDelLoteAsync(ProcesarLoteTests_Setup.LoteId, Arg.Any<CancellationToken>()).Returns([p]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(1);
        anomalias.ProcesarReporteAsync(Arg.Any<EventoReportado>(), Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>())
                 .Returns(new ResultadoProceso());
        await uc.EjecutarAsync(ProcesarLoteTests_Setup.LoteId, CancellationToken.None);
        metricas.Received(1).RegistrarRetrasoCola(Arg.Is<TimeSpan>(t => t.TotalSeconds > 0));
    }

    /// <summary>El contexto incluye ConSesion, EventosEnLaHora y RecibidoUtc (FR-013, RN-15a).</summary>
    [Fact]
    public async Task Contexto_incluye_conSesion_eventosEnHora_y_recibidoUtc()
    {
        var recibido = ProcesarLoteTests_Setup.Ahora.AddMinutes(-2);
        var p = new EventoPendiente(
            new EventoReportado(Guid.NewGuid(), recibido, -13.0, -74.0, 5.0, "Bache", 4, 0.91, "1.0",
                ProcesarLoteTests_Setup.S),
            true, recibido);
        var (uc, repo, anomalias, _, _) = ProcesarLoteTests_Setup.Crear();
        repo.RecibidosDelLoteAsync(ProcesarLoteTests_Setup.LoteId, Arg.Any<CancellationToken>()).Returns([p]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(5);
        ContextoReporte? capturado = null;
        anomalias.ProcesarReporteAsync(
            Arg.Any<EventoReportado>(),
            Arg.Do<ContextoReporte>(c => capturado = c),
            Arg.Any<CancellationToken>())
            .Returns(new ResultadoProceso());
        await uc.EjecutarAsync(ProcesarLoteTests_Setup.LoteId, CancellationToken.None);
        Assert.NotNull(capturado);
        Assert.True(capturado!.ConSesion);
        Assert.Equal(5, capturado.EventosEnLaHora);
        Assert.Equal(recibido, capturado.RecibidoUtc);
    }
}
