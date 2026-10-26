using System.Net.Http.Json;
using System.Text.Json;
using KillaUru.Identidad.Application.Usuarios;
using KillaUru.Identidad.Domain;
using KillaUru.Identidad.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>
/// D3: lo que llega a la base tras acciones reales tiene la precisión acordada (alta al día,
/// sesiones a la hora), no solo las funciones de truncado. Spec 003 · FR-039.
/// </summary>
public sealed class HorasGuardadasTests(BaseDatosPrueba bd)
{
    /// <summary>Registro, renovación, cierre y desactivación: genera dos sesiones, ambas revocadas.</summary>
    private static async Task<Guid> RecorrerCicloCompletoAsync(EscenarioIdentidad escenario)
    {
        var sesion = await escenario.RegistrarAsync(EscenarioIdentidad.CorreoNuevo());
        var renovada = await escenario.RenovarAsync(EscenarioIdentidad.TokenRenovacion(sesion));
        renovada.EnsureSuccessStatusCode();
        escenario.Autenticar(await renovada.Content.ReadFromJsonAsync<JsonElement>(EscenarioIdentidad.Ct));
        (await escenario.CerrarSesionAsync()).EnsureSuccessStatusCode();
        var id = EscenarioIdentidad.UsuarioId(sesion);
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var desactivar = scope.ServiceProvider.GetRequiredService<DesactivarUsuarioUseCase>();
        Assert.Null((await desactivar.EjecutarAsync(id, "Prueba", new(Guid.NewGuid(), "Administrador"), EscenarioIdentidad.Ct)).Error);
        return id;
    }

    /// <summary>Alta truncada al día local; creación, vencimiento y revocación de sesiones a la hora.</summary>
    [Fact]
    public async Task Ciclo_completo_guarda_la_precision_acordada()
    {
        // Arrange — hora con minutos y segundos para que cualquier falta de truncado se note
        using var escenario = new EscenarioIdentidad(bd);
        escenario.Reloj.SetUtcNow(new DateTimeOffset(2031, 2, 3, 10, 29, 31, TimeSpan.Zero));

        // Act
        var id = await RecorrerCicloCompletoAsync(escenario);

        // Assert
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
        var cuenta = await db.Usuarios.SingleAsync(x => x.Id == id, EscenarioIdentidad.Ct);
        var sesiones = await db.Sesiones.Where(x => x.UsuarioId == id).ToListAsync(EscenarioIdentidad.Ct);
        Assert.Equal(HorasGuardadas.AlDia(escenario.Reloj.GetUtcNow()), cuenta.CreadoUtc);
        Assert.Equal(2, sesiones.Count);
        Assert.All(sesiones, s => SesionTruncadaALaHora(s, escenario.Reloj.GetUtcNow()));
    }

    private static void SesionTruncadaALaHora(SesionRenovacion sesion, DateTimeOffset revocadaEn)
    {
        Assert.Equal(HorasGuardadas.ALaHora(sesion.CreadoUtc), sesion.CreadoUtc);
        Assert.Equal(HorasGuardadas.ALaHora(sesion.ExpiraUtc), sesion.ExpiraUtc);
        Assert.Equal(HorasGuardadas.ALaHora(revocadaEn), sesion.RevocadoUtc);
    }
}
