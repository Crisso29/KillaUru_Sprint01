using KillaUru.Anomalias.Contracts;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Eventos;
using KillaUru.Shared.Metricas;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>PostGIS y transacciones reales; reloj, corredor y efectos externos sintéticos.</summary>
internal sealed class EscenarioAnomalias : IDisposable
{
    private static int secuencia;
    internal FakeTimeProvider Reloj { get; } = new(new DateTimeOffset(2026, 10, 24, 12, 0, 0, TimeSpan.Zero));
    internal IPublicadorEventos Publicador { get; } = Substitute.For<IPublicadorEventos>();
    internal IMetricas Metricas { get; } = Substitute.For<IMetricas>();
    internal WebApplicationFactory<Program> Api { get; }
    internal double Longitud { get; } = -76 + Interlocked.Increment(ref secuencia) * 0.003;
    internal EscenarioAnomalias(BaseDatosPrueba bd)
    {
        Api = new FabricaApi(bd).WithWebHostBuilder(b => b.ConfigureTestServices(s =>
        {
            s.AddSingleton<TimeProvider>(Reloj); s.AddSingleton(Publicador); s.AddSingleton(Metricas);
        }));
    }
    internal EventoReportado Evento(int severidad = 2) => new(Guid.NewGuid(), Reloj.GetUtcNow(), -13, Longitud,
        2, "Bache", severidad, 0.9, "prueba", Guid.NewGuid());
    internal async Task<ResultadoProceso> ProcesarAsync(EventoReportado evento, bool sesion = false, int conteo = 1)
    {
        await using var scope = Api.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IAnomaliasService>()
            .ProcesarReporteAsync(evento, new(sesion, conteo, Reloj.GetUtcNow()), TestContext.Current.CancellationToken);
    }
    public void Dispose() => Api.Dispose();
}
