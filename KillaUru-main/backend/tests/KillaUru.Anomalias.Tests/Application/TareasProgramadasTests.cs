using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Application.Tareas;
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
/// <summary>Reinicio a las 03:29 Lima: ejecución inicial y siguiente instante exacto de cada calendario.</summary>
public sealed class TareasProgramadasTests
{
    [Theory]
    [InlineData(0,36,"caducidad")]
    [InlineData(1,41,"purga-seudonimos")]
    [InlineData(2,1,"purga-historica")]
    public async Task Arranca_y_repite_por_reloj(int tipo,int minutos,string nombre)
    {
        var reloj=new FakeTimeProvider(new DateTimeOffset(2026,10,25,8,29,0,TimeSpan.Zero));
        var metricas=Substitute.For<IMetricas>(); var veces=0;
        metricas.When(m=>m.RegistrarTareaCorrecta(nombre)).Do(_=>Interlocked.Increment(ref veces));
        using var servicios=Servicios(reloj,metricas); var scopes=servicios.GetRequiredService<IServiceScopeFactory>();
        using BackgroundService tarea=tipo switch
        {
            0=>new TareaCaducidad(scopes,reloj,NullLogger<TareaCaducidad>.Instance),
            1=>new TareaPurgaSeudonimos(scopes,reloj,NullLogger<TareaPurgaSeudonimos>.Instance),
            _=>new TareaPurgaHistorica(scopes,reloj,NullLogger<TareaPurgaHistorica>.Instance),
        };
        var ct=TestContext.Current.CancellationToken; await tarea.StartAsync(ct);
        await EsperarAsync(()=>Volatile.Read(ref veces)==1,ct);
        reloj.Advance(TimeSpan.FromMinutes(minutos)-TimeSpan.FromSeconds(1));
        await Task.Delay(30,ct); Assert.Equal(1,Volatile.Read(ref veces));
        reloj.Advance(TimeSpan.FromSeconds(1));
        await EsperarAsync(()=>Volatile.Read(ref veces)==2,ct);
        await tarea.StopAsync(ct);
    }
    private static ServiceProvider Servicios(TimeProvider reloj,IMetricas metricas)
    {
        var tx=Substitute.For<ITransaccionAnomalias>();
        tx.EjecutarAsync(Arg.Any<IReadOnlyCollection<long>>(),null,null,Arg.Any<Func<Task<int>>>(),Arg.Any<CancellationToken>())
            .Returns(c=>c.Arg<Func<Task<int>>>()());
        var repo=Substitute.For<ICaducidadRepository>();
        repo.CaducarAsync(Arg.Any<DateTimeOffset>(),Arg.Any<CancellationToken>()).Returns(Array.Empty<KillaUru.Anomalias.Contracts.AnomaliaCambiada>());
        return new ServiceCollection().AddSingleton(reloj).AddSingleton(metricas).AddSingleton(tx).AddSingleton(repo)
            .AddSingleton(Substitute.For<IPurgasRepository>()).AddSingleton(Substitute.For<IPublicadorEventos>())
            .AddSingleton(Options.Create(new AnomaliasOptions())).AddScoped<CaducarAnomaliasUseCase>()
            .AddScoped<PurgarSeudonimosUseCase>().AddScoped<PurgarHistoricoUseCase>().BuildServiceProvider();
    }
    private static async Task EsperarAsync(Func<bool> condicion,CancellationToken ct)
    {
        for(var i=0;i<200&&!condicion();i++) await Task.Delay(10,ct);
        Assert.True(condicion()); await Task.Delay(30,ct);
    }
}
