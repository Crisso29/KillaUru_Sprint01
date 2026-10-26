using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KillaUru.Identidad.Application.Usuarios;
using KillaUru.Identidad.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Identidad;

public sealed class DesactivacionRestauracionTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Desactivacion_revoca_refresh_y_jwt_caduca_en_cinco_minutos()
    {
        using var e = new EscenarioIdentidad(bd);
        var sesion = await e.RegistrarAsync(Guid.NewGuid() + "@example.com");
        var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(sesion.GetProperty("tokenAcceso").GetString());
        var id = Guid.Parse(jwt.Subject);
        await using var scope = e.Api.Services.CreateAsyncScope();
        var desactivar = scope.ServiceProvider.GetRequiredService<DesactivarUsuarioUseCase>();
        Assert.Null((await desactivar.EjecutarAsync(id, "Prueba sintética", new(Guid.NewGuid(), "Administrador"), TestContext.Current.CancellationToken)).Error);
        var renovar = await e.Cliente.PostAsJsonAsync("/api/v1/auth/renovar",
            new { tokenRenovacion = sesion.GetProperty("tokenRenovacion").GetString() }, TestContext.Current.CancellationToken);
        Assert.Equal("cuenta_desactivada", (await renovar.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("codigo").GetString());
        e.Autenticar(sesion);
        e.Reloj.Advance(TimeSpan.FromMinutes(5));
        Assert.Equal(HttpStatusCode.Unauthorized, (await e.Cliente.GetAsync("/api/v1/me", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Restauracion_revoca_toda_familia_del_respaldo()
    {
        using var e = new EscenarioIdentidad(bd);
        var sesion = await e.RegistrarAsync(Guid.NewGuid() + "@example.com");
        await using var scope = e.Api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
        var ahora = e.Reloj.GetUtcNow();
        var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(sesion.GetProperty("tokenAcceso").GetString());
        var id = Guid.Parse(jwt.Subject);
        // Acota a la cuenta sintética para no invalidar pruebas concurrentes; el runbook aplica a todas.
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE identidad.sesion_refresh SET revocado_utc = date_trunc('hour', {ahora})
            WHERE usuario_id = {id} AND revocado_utc IS NULL
            """, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, (await e.Cliente.PostAsJsonAsync("/api/v1/auth/renovar",
            new { tokenRenovacion = sesion.GetProperty("tokenRenovacion").GetString() }, TestContext.Current.CancellationToken)).StatusCode);
    }
}
