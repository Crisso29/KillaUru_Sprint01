using System.Net;
using KillaUru.Anomalias.Application.Procesamiento;
using KillaUru.Anomalias.Contracts;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Domain;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>La disponibilidad HTTP impide aceptar tráfico antes de cargar el corredor.</summary>
[Collection("Anomalias")]
public sealed class DisponibilidadCorredorTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Readiness_pasa_de_503_a_200_y_las_celdas_aparecen_al_cargar()
    {
        var estado=new EstadoCorredor();
        using var api=new FabricaApi(bd).WithWebHostBuilder(b=>b.ConfigureTestServices(s=>s.AddSingleton(estado)));
        using var cliente=api.CreateClient(); var ct=TestContext.Current.CancellationToken;
        using var antes=await cliente.GetAsync("/health/ready",ct); Assert.Equal(HttpStatusCode.ServiceUnavailable,antes.StatusCode);
        await using var scope=api.Services.CreateAsyncScope(); var lectura=scope.ServiceProvider.GetRequiredService<IAnomaliaLectura>();
        Assert.Empty(lectura.CeldasDelCorredor());
        estado.Cargar(new([Coordenada.Crear(-13,-76)!.Value,Coordenada.Crear(-13,-75)!.Value],50));
        using var despues=await cliente.GetAsync("/health/ready",ct); Assert.Equal(HttpStatusCode.OK,despues.StatusCode);
        Assert.NotEmpty(lectura.CeldasDelCorredor());
    }
}
