using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Application.Tareas;
using KillaUru.Anomalias.Contracts;
using KillaUru.Shared.Domain;
using KillaUru.Shared.Eventos;
using KillaUru.Shared.Metricas;
using NSubstitute;

namespace KillaUru.Anomalias.Tests.Application;

/// <summary>
/// La tarea de caducidad avisa al mapa solo después del commit, para que un rollback nunca invalide
/// la caché, y solo entonces declara éxito. Spec 004 · FR-014, FR-018, FR-024.
/// </summary>
public sealed class CaducidadTests
{
    private readonly IPublicadorEventos _eventos = Substitute.For<IPublicadorEventos>();
    private readonly List<Func<Task>> _alConfirmar = [];
    private bool _publicadoAntesDelCommit;

    /// <summary>Ejecuta la acción, anota si ya se había publicado algo y recién entonces corre los efectos.</summary>
    private ITransaccionAnomalias TransaccionQueConfirma()
    {
        var transaccion = Substitute.For<ITransaccionAnomalias>();
        transaccion.AlConfirmar(Arg.Do<Func<Task>>(_alConfirmar.Add));
        transaccion.EjecutarAsync(Arg.Any<IReadOnlyCollection<long>>(), null, null, Arg.Any<Func<Task<int>>>(), Arg.Any<CancellationToken>())
            .Returns(async llamada =>
            {
                var caducadas = await llamada.Arg<Func<Task<int>>>()();
                _publicadoAntesDelCommit = _eventos.ReceivedCalls().Any();
                foreach (var efecto in _alConfirmar) await efecto();
                return caducadas;
            });
        return transaccion;
    }

    /// <summary>Cada anomalía caducada se publica una vez, después del commit, y la tarea registra su éxito.</summary>
    [Fact]
    public async Task Publica_despues_del_commit_y_registra_exito()
    {
        // Arrange
        var repositorio = Substitute.For<ICaducidadRepository>();
        var metricas = Substitute.For<IMetricas>();
        var cambio = new AnomaliaCambiada(Guid.NewGuid(), Celda.De(Coordenada.Crear(0, 0)!.Value));
        repositorio.CaducarAsync(Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns([cambio]);
        var caso = new CaducarAnomaliasUseCase(repositorio, TransaccionQueConfirma(), _eventos, metricas, TimeProvider.System);

        // Act
        await caso.EjecutarAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.False(_publicadoAntesDelCommit);
        await _eventos.Received(1).PublicarAsync(cambio, Arg.Any<CancellationToken>());
        metricas.Received(1).RegistrarTareaCorrecta("caducidad");
    }
}
