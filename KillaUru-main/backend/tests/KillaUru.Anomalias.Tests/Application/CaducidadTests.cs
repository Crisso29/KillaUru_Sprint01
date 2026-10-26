using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Application.Tareas;
using KillaUru.Anomalias.Contracts;
using KillaUru.Shared.Eventos;
using KillaUru.Shared.Metricas;
using NSubstitute;
namespace KillaUru.Anomalias.Tests.Application;
/// <summary>Un rollback nunca invalida el mapa ni declara éxito de una tarea.</summary>
public sealed class CaducidadTests
{
    [Fact]
    public async Task Publica_despues_del_commit_y_registra_exito()
    {
        var repo = Substitute.For<ICaducidadRepository>(); var tx = Substitute.For<ITransaccionAnomalias>();
        var eventos = Substitute.For<IPublicadorEventos>(); var metricas = Substitute.For<IMetricas>();
        var cambio = new AnomaliaCambiada(Guid.NewGuid(), KillaUru.Shared.Domain.Celda.De(KillaUru.Shared.Domain.Coordenada.Crear(0, 0)!.Value)); var pendientes = new List<Func<Task>>();
        repo.CaducarAsync(Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(new[] { cambio });
        tx.AlConfirmar(Arg.Do<Func<Task>>(pendientes.Add));
        tx.EjecutarAsync(Arg.Any<IReadOnlyCollection<long>>(), null, null, Arg.Any<Func<Task<int>>>(), Arg.Any<CancellationToken>())
            .Returns(async llamada => { var n = await llamada.Arg<Func<Task<int>>>()();
                await eventos.DidNotReceive().PublicarAsync(cambio, Arg.Any<CancellationToken>());
                foreach (var accion in pendientes) await accion(); return n; });
        await new CaducarAnomaliasUseCase(repo, tx, eventos, metricas, TimeProvider.System).EjecutarAsync(TestContext.Current.CancellationToken);
        await eventos.Received(1).PublicarAsync(cambio, Arg.Any<CancellationToken>());
        metricas.Received(1).RegistrarTareaCorrecta("caducidad");
    }
}

