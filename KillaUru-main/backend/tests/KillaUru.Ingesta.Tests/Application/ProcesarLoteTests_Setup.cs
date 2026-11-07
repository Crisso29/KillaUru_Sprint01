using KillaUru.Anomalias.Contracts;
using KillaUru.Ingesta.Application.Procesamiento;
using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace KillaUru.Ingesta.Tests.Application;

internal static class ProcesarLoteTests_Setup
{
    internal static readonly Guid LoteId = Guid.NewGuid();
    internal static readonly Guid S = Guid.NewGuid();
    internal static readonly DateTimeOffset Ahora = new(2026, 10, 20, 14, 32, 05, TimeSpan.Zero);

    internal static EventoPendiente Pendiente(DateTimeOffset detectado, bool conSesion = false) =>
        new(new EventoReportado(Guid.NewGuid(), detectado, -13.0, -74.0, 5.0, "Bache", 4, 0.91, "1.0", S),
            conSesion, Ahora.AddMinutes(-2));

    internal static (ProcesarLoteUseCase uc, IPendientesRepositorio repo,
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
}
