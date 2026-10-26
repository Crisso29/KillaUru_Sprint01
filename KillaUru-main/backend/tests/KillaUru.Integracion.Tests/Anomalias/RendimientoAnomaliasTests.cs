using System.Collections.Concurrent;
using System.Text.Json;
using KillaUru.Anomalias.Contracts;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Metricas;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>Medición local del procesador con 300 dispositivos y ocho trabajadores; no certifica la futura cola 005.</summary>
[Collection("Anomalias")]
public sealed class RendimientoAnomaliasTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Mide_300_dispositivos_con_reloj_real()
    {
        var duraciones=new ConcurrentBag<double>(); var metricas=Substitute.For<IMetricas>();
        metricas.When(m=>m.RegistrarDuracionProceso(Arg.Any<TimeSpan>())).Do(c=>duraciones.Add(c.Arg<TimeSpan>().TotalMilliseconds));
        var corredor=new KillaUru.Anomalias.Application.Procesamiento.EstadoCorredor();
        corredor.Cargar(new([KillaUru.Shared.Domain.Coordenada.Crear(-12,-76)!.Value,KillaUru.Shared.Domain.Coordenada.Crear(-12,-75)!.Value],50));
        using var api=new FabricaApi(bd).WithWebHostBuilder(b=>b.ConfigureTestServices(s=> { s.AddSingleton(metricas);s.AddSingleton(corredor); }));
        await ProcesarAsync(api.Services,0,TestContext.Current.CancellationToken); duraciones.Clear();
        await Parallel.ForEachAsync(Enumerable.Range(1,300),new ParallelOptions { MaxDegreeOfParallelism=8, CancellationToken=TestContext.Current.CancellationToken },
            (i,ct)=>new ValueTask(ProcesarAsync(api.Services,i,ct)));
        var muestras=duraciones.Order().ToArray(); Assert.Equal(300,muestras.Length);
        var resultado=new { dispositivos=300, trabajadores=8, p95Ms=muestras[284], maxMs=muestras[^1], mediaMs=muestras.Average(), cola005=false };
        await File.WriteAllTextAsync(Path.Combine(Rutas.Backend,"TestResults/004-rendimiento.json"),JsonSerializer.Serialize(resultado),TestContext.Current.CancellationToken);
    }
    private static async Task ProcesarAsync(IServiceProvider servicios,int i,CancellationToken ct)
    {
        await using var scope=servicios.CreateAsyncScope(); var ahora=TimeProvider.System.GetUtcNow();
        var evento=new EventoReportado(Guid.NewGuid(),ahora,-12,-75.95+i*.002,2,"Bache",2,.9,"prueba-rendimiento",Guid.NewGuid());
        var r=await scope.ServiceProvider.GetRequiredService<IAnomaliasService>().ProcesarReporteAsync(evento,new(false,1,ahora),ct);
        Assert.Null(r.Motivo); Assert.False(r.YaProcesado);
    }
}
