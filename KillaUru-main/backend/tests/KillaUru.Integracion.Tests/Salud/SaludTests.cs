using System.Net;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KillaUru.Integracion.Tests.Salud;

/// <summary>T045 (RF-28, FR-018, FR-019): vivo sin BD, listo con BD y migraciones, y cada módulo suma su chequeo.</summary>
public sealed class SaludTests(BaseDatosPrueba bd)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Live_y_ready_dan_200_con_la_BD_y_las_migraciones_al_dia()
    {
        using var api = new FabricaApi(bd);
        var cliente = api.CreateClient();

        Assert.Equal("Healthy", await cliente.GetStringAsync("/health/live", Ct));
        Assert.Equal("Healthy", await cliente.GetStringAsync("/health/ready", Ct));
    }

    [Fact]
    public async Task Ready_da_503_si_no_hay_base_de_datos_y_live_sigue_en_200()
    {
        using var api = new FabricaApi(bd).WithWebHostBuilder(b =>
            b.UseSetting("ConnectionStrings:KillaUru", "Host=127.0.0.1;Port=1;Database=killauru;Username=x;Password=x;Timeout=1"));
        var cliente = api.CreateClient();

        using var listo = await cliente.GetAsync("/health/ready", Ct);
        using var vivo = await cliente.GetAsync("/health/live", Ct);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, listo.StatusCode);
        Assert.Equal("Unhealthy", await listo.Content.ReadAsStringAsync(Ct));   // sin datos internos
        Assert.Equal(HttpStatusCode.OK, vivo.StatusCode);
    }

    [Fact]
    public async Task El_chequeo_de_un_modulo_se_suma_a_ready()
    {
        using var api = new FabricaApi(bd).WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddHealthChecks()
            .AddCheck("modulo-de-prueba", () => HealthCheckResult.Unhealthy(), tags: ["ready"])));

        using var listo = await api.CreateClient().GetAsync("/health/ready", Ct);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, listo.StatusCode);
    }
}
