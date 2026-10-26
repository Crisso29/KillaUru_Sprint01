using KillaUru.Anomalias.Application.Procesamiento;
using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Domain;
using KillaUru.Shared.Eventos;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace KillaUru.Anomalias.Tests.Application;

/// <summary>
/// Dobles del procesamiento. La transacción guarda los efectos «tras confirmar» y los ejecuta solo si
/// el commit sale bien; mientras tanto anota si algo se publicó antes de tiempo.
/// </summary>
internal sealed class EscenarioProcesamiento
{
    internal static readonly DateTimeOffset Ahora = new(2026, 10, 25, 12, 0, 0, TimeSpan.Zero);

    internal IAnomaliaRepository Anomalias { get; } = Substitute.For<IAnomaliaRepository>();
    internal IPublicadorEventos Eventos { get; } = Substitute.For<IPublicadorEventos>();
    internal IMetricas Metricas { get; } = Substitute.For<IMetricas>();
    internal bool HuboEfectosAntesDelCommit { get; private set; }

    private readonly ITransaccionAnomalias _transaccion = Substitute.For<ITransaccionAnomalias>();
    private readonly List<Func<Task>> _alConfirmar = [];

    internal EscenarioProcesamiento(bool commitFalla = false)
    {
        _transaccion.AlConfirmar(Arg.Do<Func<Task>>(_alConfirmar.Add));
        _transaccion.EjecutarAsync(Arg.Any<IReadOnlyCollection<long>>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(),
                Arg.Any<Func<Task<ResultadoProceso>>>(), Arg.Any<CancellationToken>())
            .Returns(async llamada =>
            {
                var resultado = await llamada.Arg<Func<Task<ResultadoProceso>>>()();
                HuboEfectosAntesDelCommit = Publicaciones > 0 || Metricas.ReceivedCalls().Any(c => c.GetMethodInfo().Name == nameof(IMetricas.RegistrarRetenidoPorTope));
                if (commitFalla) throw new InvalidOperationException("commit fallido");
                foreach (var efecto in _alConfirmar) await efecto();
                return resultado;
            });
    }

    internal int Publicaciones => Eventos.ReceivedCalls().Count(c => c.GetMethodInfo().Name == nameof(IPublicadorEventos.PublicarAsync));

    /// <summary>Procesa un evento leve cuyo seudónimo ya envió 121 en esa hora (supera el tope de 120).</summary>
    internal Task<ResultadoProceso> ProcesarConTopeSuperadoAsync()
    {
        var reloj = new FakeTimeProvider(Ahora);
        var caso = new ProcesarReporteUseCase(Anomalias, Substitute.For<IReporteRepository>(), _transaccion, Corredor(),
            Options.Create(new AnomaliasOptions()), Eventos, Metricas, reloj);
        return caso.EjecutarAsync(Evento(Ahora), new ContextoReporte(false, 121, Ahora), TestContext.Current.CancellationToken);
    }

    internal static EventoReportado Evento(DateTimeOffset detectado) =>
        new(Guid.NewGuid(), detectado, 0, 0, 1, "Bache", 2, 0.9, "prueba", Guid.NewGuid());

    /// <summary>Recta de unos 2,2 km sobre el ecuador, ya cargada como al arrancar la API.</summary>
    internal static EstadoCorredor Corredor()
    {
        var estado = new EstadoCorredor();
        estado.Cargar(new([Coordenada.Crear(0, -0.01)!.Value, Coordenada.Crear(0, 0.01)!.Value], 50));
        return estado;
    }
}
