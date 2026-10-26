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
/// <summary>La recepción original gobierna la calidad y los efectos externos dependen del commit.</summary>
public sealed class ProcesamientoUseCaseTests
{
    [Fact]
    public void Evaluacion_intrinseca_es_pura_y_usa_recepcion_original()
    {
        var recibido=new DateTimeOffset(2026,10,25,12,0,0,TimeSpan.Zero); var e=Evento(recibido.AddDays(-7));
        var useCase=new EvaluarIntrinsecaUseCase(Corredor(),Options.Create(new AnomaliasOptions()));
        Assert.Null(useCase.Ejecutar(e,recibido));
        Assert.Equal(KillaUru.Anomalias.Contracts.MotivoDescarte.TimestampFueraDeRango,useCase.Ejecutar(e,recibido.AddSeconds(1)));
        Assert.Equal(KillaUru.Anomalias.Contracts.MotivoDescarte.EsquemaInvalido,useCase.Ejecutar(e with {ModeloVersion=null},recibido));
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Commit_gobierna_publicacion_y_tope(bool fallo)
    {
        var repo=Substitute.For<IAnomaliaRepository>(); var reportes=Substitute.For<IReporteRepository>();
        var tx=Substitute.For<ITransaccionAnomalias>(); var eventos=Substitute.For<IPublicadorEventos>(); var metricas=Substitute.For<IMetricas>();
        var pendientes=new List<Func<Task>>();tx.AlConfirmar(Arg.Do<Func<Task>>(pendientes.Add));
        tx.EjecutarAsync(Arg.Any<IReadOnlyCollection<long>>(),Arg.Any<Guid?>(),Arg.Any<Guid?>(),Arg.Any<Func<Task<ResultadoProceso>>>(),Arg.Any<CancellationToken>())
            .Returns(async c=> { var r=await c.Arg<Func<Task<ResultadoProceso>>>()();
                Assert.DoesNotContain(eventos.ReceivedCalls(),x=>x.GetMethodInfo().Name=="PublicarAsync");
                metricas.DidNotReceive().RegistrarRetenidoPorTope();
                if(fallo) throw new InvalidOperationException("commit fallido");
                foreach(var accion in pendientes) await accion();return r; });
        var reloj=new FakeTimeProvider(new DateTimeOffset(2026,10,25,12,0,0,TimeSpan.Zero));
        var useCase=new ProcesarReporteUseCase(repo,reportes,tx,Corredor(),Options.Create(new AnomaliasOptions()),eventos,metricas,reloj);
        var ejecutar=()=>useCase.EjecutarAsync(Evento(reloj.GetUtcNow()),new(false,121,reloj.GetUtcNow()),TestContext.Current.CancellationToken);
        if(fallo) await Assert.ThrowsAsync<InvalidOperationException>(ejecutar); else Assert.Null((await ejecutar()).Motivo);
        Assert.Equal(fallo?0:1,eventos.ReceivedCalls().Count(c=>c.GetMethodInfo().Name=="PublicarAsync"));
        metricas.Received(fallo?0:1).RegistrarRetenidoPorTope();metricas.Received(1).RegistrarDuracionProceso(Arg.Any<TimeSpan>());
        await repo.Received(1).GuardarAsync(Arg.Is<Anomalia>(a=>a.Estado==KillaUru.Anomalias.Domain.EstadoAnomalia.PendienteConfirmacion),true,Arg.Any<CancellationToken>());
    }
    private static EventoReportado Evento(DateTimeOffset instante)=>new(Guid.NewGuid(),instante,0,0,1,"Bache",2,.9,"prueba",Guid.NewGuid());
    private static EstadoCorredor Corredor()
    {
        var estado=new EstadoCorredor();estado.Cargar(new([Coordenada.Crear(0,-.01)!.Value,Coordenada.Crear(0,.01)!.Value],50));return estado;
    }
}
