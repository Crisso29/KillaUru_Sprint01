using KillaUru.Anomalias.Contracts;
using KillaUru.Dashboard.Application;
using KillaUru.Dashboard.Application.Puertos;
using KillaUru.Dashboard.Domain;
using KillaUru.Identidad.Contracts;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Metricas;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace KillaUru.Integracion.Tests.Dashboard;

/// <summary>
/// Base de datos real (Testcontainers); reloj y contratos externos sustituidos.
/// </summary>
internal sealed class EscenarioDashboard : IDisposable
{
    internal FakeTimeProvider          Reloj     { get; } = new(new DateTimeOffset(2026, 10, 21, 10, 0, 0, TimeSpan.Zero));
    internal IAnomaliaLectura          Anomalias { get; } = Substitute.For<IAnomaliaLectura>();
    internal IIdentidadConsultas       Identidad { get; } = Substitute.For<IIdentidadConsultas>();
    internal IMetricas                 Metricas  { get; } = Substitute.For<IMetricas>();
    internal WebApplicationFactory<Program> Api { get; }
    internal static CancellationToken  Ct        => TestContext.Current.CancellationToken;

    internal EscenarioDashboard(BaseDatosPrueba bd)
    {
        Api = new FabricaApi(bd).WithWebHostBuilder(b => b.ConfigureTestServices(s =>
        {
            s.AddSingleton<TimeProvider>(Reloj);
            s.AddSingleton(Anomalias);
            s.AddSingleton(Identidad);
            s.AddSingleton(Metricas);
            foreach (var t in s.Where(x => x.ImplementationType == typeof(KillaUru.Dashboard.Infrastructure.TareaIndicadoresDiarios)).ToArray())
                s.Remove(t);
        }));
    }

    internal async Task GuardarDiaAsync(DiaIndicadores dia)
    {
        await using var scope = Api.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IIndicadoresRepositorio>().GuardarDiaAsync(dia, Ct);
    }

    internal static DiaIndicadores DiaSintetico(DateOnly fecha, long ev = 300, long ca = 40, long dd = 10,
        int t7 = 237, int tt = 336, long conf = 18, long pend = 6, long exp = 3, long renac = 2) =>
        new(fecha, ev, ca, dd, t7, tt, conf, pend, exp, renac);

    public void Dispose() => Api.Dispose();
}
