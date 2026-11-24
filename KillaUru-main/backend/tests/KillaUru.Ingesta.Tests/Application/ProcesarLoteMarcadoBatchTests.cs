using KillaUru.Anomalias.Contracts;
using KillaUru.Ingesta.Application.Procesamiento;
using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Ingesta.Domain;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace KillaUru.Ingesta.Tests.Application;

/// <summary>
/// ProcesarLoteUseCase marca todos los eventos exitosos en un único commit al final del lote,
/// no uno por uno (DT-01: elimina N commits WAL por lote → 1 commit WAL).
/// Spec 005 · T019 · Fix A.
/// </summary>
public sealed class ProcesarLoteMarcadoBatchTests
{
    [Fact]
    public async Task Lote_llama_marcar_lote_una_sola_vez_para_n_eventos()
    {
        // Arrange
        var p1 = ProcesarLoteTests_Setup.Pendiente(ProcesarLoteTests_Setup.Ahora.AddMinutes(-10));
        var p2 = ProcesarLoteTests_Setup.Pendiente(ProcesarLoteTests_Setup.Ahora.AddMinutes(-5));
        var p3 = ProcesarLoteTests_Setup.Pendiente(ProcesarLoteTests_Setup.Ahora.AddMinutes(-3));
        var (uc, repo, _, _, _) = ProcesarLoteTests_Setup.Crear();
        repo.RecibidosDelLoteAsync(ProcesarLoteTests_Setup.LoteId, Arg.Any<CancellationToken>()).Returns([p1, p2, p3]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(1);

        // Act
        await uc.EjecutarAsync(ProcesarLoteTests_Setup.LoteId, CancellationToken.None);

        // Assert — un único commit para los 3 eventos, no 3 commits separados (DT-01)
        await repo.Received(1).MarcarLoteAsync(
            Arg.Is<IReadOnlyList<MarcaEvento>>(m => m.Count == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Excepcion_no_incluye_el_evento_fallido_en_el_lote()
    {
        // Arrange — p1 lanza excepción, p2 y p3 se procesan bien
        var p1 = ProcesarLoteTests_Setup.Pendiente(ProcesarLoteTests_Setup.Ahora.AddMinutes(-10));
        var p2 = ProcesarLoteTests_Setup.Pendiente(ProcesarLoteTests_Setup.Ahora.AddMinutes(-5));
        var p3 = ProcesarLoteTests_Setup.Pendiente(ProcesarLoteTests_Setup.Ahora.AddMinutes(-3));
        var (uc, repo, anomalias, _, _) = ProcesarLoteTests_Setup.Crear();
        repo.RecibidosDelLoteAsync(ProcesarLoteTests_Setup.LoteId, Arg.Any<CancellationToken>()).Returns([p1, p2, p3]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(1);
        anomalias.ProcesarReporteAsync(
            Arg.Is<EventoReportado>(e => e.EventoId == p1.Evento.EventoId),
            Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("fallo simulado"));

        // Act
        await uc.EjecutarAsync(ProcesarLoteTests_Setup.LoteId, CancellationToken.None);

        // Assert — solo p2 y p3 en el batch; p1 queda Recibido para el barredor (R-015)
        await repo.Received(1).MarcarLoteAsync(
            Arg.Is<IReadOnlyList<MarcaEvento>>(m =>
                m.Count == 2 &&
                m.All(x => x.EventoId != p1.Evento.EventoId)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Lote_sin_eventos_no_llama_marcar_lote()
    {
        // Arrange
        var (uc, repo, _, _, _) = ProcesarLoteTests_Setup.Crear();
        repo.RecibidosDelLoteAsync(ProcesarLoteTests_Setup.LoteId, Arg.Any<CancellationToken>()).Returns([]);

        // Act
        await uc.EjecutarAsync(ProcesarLoteTests_Setup.LoteId, CancellationToken.None);

        // Assert
        await repo.DidNotReceive().MarcarLoteAsync(
            Arg.Any<IReadOnlyList<MarcaEvento>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Descartado_incluye_estado_descartado_y_motivo_en_la_marca()
    {
        // Arrange
        var p = ProcesarLoteTests_Setup.Pendiente(ProcesarLoteTests_Setup.Ahora.AddMinutes(-5));
        var (uc, repo, anomalias, _, _) = ProcesarLoteTests_Setup.Crear();
        repo.RecibidosDelLoteAsync(ProcesarLoteTests_Setup.LoteId, Arg.Any<CancellationToken>()).Returns([p]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(1);
        anomalias.ProcesarReporteAsync(Arg.Any<EventoReportado>(), Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>())
                 .Returns(new ResultadoProceso(Motivo: MotivoDescarte.FueraDelCorredor));

        // Act
        await uc.EjecutarAsync(ProcesarLoteTests_Setup.LoteId, CancellationToken.None);

        // Assert
        await repo.Received(1).MarcarLoteAsync(
            Arg.Is<IReadOnlyList<MarcaEvento>>(m =>
                m.Count == 1 &&
                m[0].Estado == EstadoRecepcion.Descartado &&
                m[0].Motivo == (short)MotivoDescarte.FueraDelCorredor),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Ya_procesado_se_incluye_en_el_lote_como_procesado()
    {
        // Arrange
        var p = ProcesarLoteTests_Setup.Pendiente(ProcesarLoteTests_Setup.Ahora.AddMinutes(-5));
        var (uc, repo, anomalias, _, _) = ProcesarLoteTests_Setup.Crear();
        repo.RecibidosDelLoteAsync(ProcesarLoteTests_Setup.LoteId, Arg.Any<CancellationToken>()).Returns([p]);
        repo.ContarEnLaHoraAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(1);
        anomalias.ProcesarReporteAsync(Arg.Any<EventoReportado>(), Arg.Any<ContextoReporte>(), Arg.Any<CancellationToken>())
                 .Returns(new ResultadoProceso(YaProcesado: true));

        // Act
        await uc.EjecutarAsync(ProcesarLoteTests_Setup.LoteId, CancellationToken.None);

        // Assert — idempotencia: ya procesado también se marca Procesado en el batch
        await repo.Received(1).MarcarLoteAsync(
            Arg.Is<IReadOnlyList<MarcaEvento>>(m =>
                m.Count == 1 && m[0].Estado == EstadoRecepcion.Procesado && m[0].Motivo == null),
            Arg.Any<CancellationToken>());
    }
}
