using System.Net.Http.Json;
using System.Text.Json;
using KillaUru.Identidad.Application.Usuarios;
using KillaUru.Identidad.Domain;
using KillaUru.Identidad.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>Verifica los valores persistidos tras acciones reales, además de las funciones de truncado.</summary>
public sealed class HorasGuardadasTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Registro_rotacion_cierre_y_desactivacion_guardan_precision_acordada()
    {
        using var e = new EscenarioIdentidad(bd);
        e.Reloj.SetUtcNow(new DateTimeOffset(2031, 2, 3, 10, 29, 31, TimeSpan.Zero));
        var sesion = await e.RegistrarAsync(Guid.NewGuid() + "@example.com");
        var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(sesion.GetProperty("tokenAcceso").GetString());
        var id = Guid.Parse(jwt.Subject);
        var respuesta = await e.Cliente.PostAsJsonAsync("/api/v1/auth/renovar", new { tokenRenovacion = sesion.GetProperty("tokenRenovacion").GetString() }, TestContext.Current.CancellationToken);
        respuesta.EnsureSuccessStatusCode();
        e.Autenticar(await respuesta.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken));
        (await e.Cliente.PostAsync("/api/v1/auth/cerrar-sesion", null, TestContext.Current.CancellationToken)).EnsureSuccessStatusCode();
        await using var scope = e.Api.Services.CreateAsyncScope();
        var desactivar = scope.ServiceProvider.GetRequiredService<DesactivarUsuarioUseCase>();
        Assert.Null((await desactivar.EjecutarAsync(id, "Prueba", new(Guid.NewGuid(), "Administrador"), TestContext.Current.CancellationToken)).Error);
        var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
        var cuenta = await db.Usuarios.SingleAsync(x => x.Id == id, TestContext.Current.CancellationToken);
        Assert.Equal(HorasGuardadas.AlDia(e.Reloj.GetUtcNow()), cuenta.CreadoUtc);
        var sesiones = await db.Sesiones.Where(x => x.UsuarioId == id).ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, sesiones.Count);
        Assert.All(sesiones, s =>
        {
            Assert.Equal(HorasGuardadas.ALaHora(s.CreadoUtc), s.CreadoUtc);
            Assert.Equal(HorasGuardadas.ALaHora(s.ExpiraUtc), s.ExpiraUtc);
            Assert.Equal(HorasGuardadas.ALaHora(e.Reloj.GetUtcNow()), s.RevocadoUtc);
        });
    }
}
