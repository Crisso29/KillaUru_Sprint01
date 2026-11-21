using System.Net;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KillaUru.Integracion.Tests.Salud;

/// <summary>
/// Spec 001 — FR-018, FR-019 (T045, RF-28, T-OPS19):
/// /health/live indica que el proceso responde (Docker lo usa como healthcheck);
/// /health/ready indica que la BD y las migraciones están al día.
/// Cada módulo puede sumar su propio chequeo sin que el host conozca su interior (ADR-002).
/// </summary>
public sealed class SaludTests(BaseDatosPrueba bd)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Live_y_ready_responden_200_Healthy_con_BD_operativa_y_migraciones_al_dia()
    {
        // Arrange — API apuntando a la BD de prueba con V0000–V0003 aplicadas
        using var api = new FabricaApi(bd);
        var cliente = api.CreateClient();

        // Act
        var estadoVivo = await cliente.GetStringAsync("/health/live", Ct);
        var estadoListo = await cliente.GetStringAsync("/health/ready", Ct);

        // Assert — ambos endpoints responden el texto "Healthy" sin datos internos (RF-28, FR-018)
        Assert.Equal("Healthy", estadoVivo);
        Assert.Equal("Healthy", estadoListo);
    }

    [Fact]
    public async Task Ready_es_503_Unhealthy_si_la_BD_no_responde_pero_live_sigue_en_200()
    {
        // Arrange — cadena de conexión apunta a un puerto cerrado (simula BD caída)
        using var api = new FabricaApi(bd).WithWebHostBuilder(b => b.UseSetting(
            "ConnectionStrings:KillaUru",
            "Host=127.0.0.1;Port=1;Database=killauru;Username=x;Password=x;Timeout=1"));
        var cliente = api.CreateClient();

        // Act
        using var listo = await cliente.GetAsync("/health/ready", Ct);
        using var vivo  = await cliente.GetAsync("/health/live",  Ct);

        // Assert — ready falla con 503 y texto plano sin stack; live sigue en 200 (FR-018, T-OPS19)
        Assert.Equal(HttpStatusCode.ServiceUnavailable, listo.StatusCode);
        Assert.Equal("Unhealthy", await listo.Content.ReadAsStringAsync(Ct));
        Assert.Equal(HttpStatusCode.OK, vivo.StatusCode);
    }

    [Fact]
    public async Task Un_modulo_puede_agregar_su_chequeo_a_ready_sin_modificar_el_host()
    {
        // Arrange — registrar un chequeo externo que devuelve Unhealthy (simula módulo degradado)
        using var api = new FabricaApi(bd).WithWebHostBuilder(b => b.ConfigureTestServices(s =>
            s.AddHealthChecks().AddCheck("modulo-de-prueba", () => HealthCheckResult.Unhealthy(), tags: ["ready"])));

        // Act
        using var listo = await api.CreateClient().GetAsync("/health/ready", Ct);

        // Assert — un solo chequeo en fallo degrada toda la respuesta de /health/ready (FR-019, ADR-002)
        Assert.Equal(HttpStatusCode.ServiceUnavailable, listo.StatusCode);
    }
}
