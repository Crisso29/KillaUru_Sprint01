using System.Net;
using System.Net.Http.Headers;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Domain;

namespace KillaUru.Integracion.Tests.Dashboard;

/// <summary>SC-011 / T019: seguridad y validación del endpoint GET /api/v1/dashboard/indicadores.</summary>
public sealed class DashboardApiTests(BaseDatosPrueba bd)
{
    private const string Ruta = "/api/v1/dashboard/indicadores";

    [Fact]
    public async Task Sin_token_devuelve_401()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var cliente   = api.CreateClient();

        // Act
        var r = await cliente.GetAsync($"{Ruta}?dias=7", EscenarioDashboard.Ct);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Token_de_conductor_devuelve_403()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var cliente   = api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Tokens.Emitir(Roles.ConductorRegistrado));

        // Act
        var r = await cliente.GetAsync($"{Ruta}?dias=7", EscenarioDashboard.Ct);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Dias_no_permitido_devuelve_400()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var cliente   = api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Tokens.Emitir("Administrador"));

        // Act
        var r = await cliente.GetAsync($"{Ruta}?dias=10", EscenarioDashboard.Ct);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Administrador_con_dias_valido_devuelve_200()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var cliente   = api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Tokens.Emitir("Administrador"));

        // Act
        var r = await cliente.GetAsync($"{Ruta}?dias=7", EscenarioDashboard.Ct);

        // Assert
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }
}
