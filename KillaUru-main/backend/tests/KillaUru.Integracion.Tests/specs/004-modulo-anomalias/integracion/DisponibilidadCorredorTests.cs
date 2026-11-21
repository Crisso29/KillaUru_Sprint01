using System.Net;
using KillaUru.Anomalias.Application.Procesamiento;
using KillaUru.Anomalias.Contracts;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Domain;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// Mientras la polilínea no está cargada, /health/ready responde 503 y el balanceador no envía
/// tráfico; al cargarla pasa a 200 y las celdas del corredor aparecen. Spec 004 · FR-033.
/// </summary>
[Collection("Anomalias")]
public sealed class DisponibilidadCorredorTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Readiness_pasa_de_503_a_200_y_las_celdas_aparecen_al_cargar()
    {
        // Arrange — el corredor se inyecta vacío para controlar cuándo se carga
        var corredor = new EstadoCorredor();
        using var api = new FabricaApi(bd).WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddSingleton(corredor)));
        using var cliente = api.CreateClient();
        await using var scope = api.Services.CreateAsyncScope();
        var lectura = scope.ServiceProvider.GetRequiredService<IAnomaliaLectura>();
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await cliente.GetAsync("/health/ready", EscenarioAnomalias.Ct)).StatusCode);
        Assert.Empty(lectura.CeldasDelCorredor());

        // Act
        corredor.Cargar(new([Coordenada.Crear(-13, -76)!.Value, Coordenada.Crear(-13, -75)!.Value], 50));

        // Assert
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/health/ready", EscenarioAnomalias.Ct)).StatusCode);
        Assert.NotEmpty(lectura.CeldasDelCorredor());
    }
}
