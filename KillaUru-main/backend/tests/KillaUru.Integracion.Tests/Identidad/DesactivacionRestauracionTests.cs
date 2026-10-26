using System.Net;
using KillaUru.Identidad.Application.Usuarios;
using KillaUru.Identidad.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>
/// Cortar el acceso de una cuenta: la desactivación revoca la renovación al instante y el JWT vivo
/// muere en 5 min; tras restaurar un respaldo se revocan todas las familias.
/// Spec 003 · FR-017, FR-019, FR-041.
/// </summary>
public sealed class DesactivacionRestauracionTests(BaseDatosPrueba bd)
{
    /// <summary>
    /// Tras desactivar, renovar responde cuenta_desactivada (la app sabe que debe cerrar la sesión)
    /// y el JWT que ya tenía deja de servir al cumplir 5 min (FR-017, FR-019).
    /// </summary>
    [Fact]
    public async Task Desactivar_corta_la_renovacion_y_el_jwt_muere_en_cinco_minutos()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var sesion = await escenario.RegistrarAsync(EscenarioIdentidad.CorreoNuevo());
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var desactivar = scope.ServiceProvider.GetRequiredService<DesactivarUsuarioUseCase>();

        // Act
        var resultado = await desactivar.EjecutarAsync(EscenarioIdentidad.UsuarioId(sesion), "Prueba sintética",
            new(Guid.NewGuid(), "Administrador"), EscenarioIdentidad.Ct);

        // Assert
        Assert.Null(resultado.Error);
        var renovar = await escenario.RenovarAsync(EscenarioIdentidad.TokenRenovacion(sesion));
        Assert.Equal("cuenta_desactivada", await EscenarioIdentidad.CodigoAsync(renovar));
        escenario.Autenticar(sesion);
        escenario.Reloj.Advance(TimeSpan.FromMinutes(5));
        Assert.Equal(HttpStatusCode.Unauthorized, (await escenario.MisDatosAsync()).StatusCode);
    }

    /// <summary>
    /// El runbook de restauración revoca las sesiones del respaldo para que nadie reuse un token
    /// que en producción ya estaba revocado (FR-041).
    /// </summary>
    [Fact]
    public async Task Restauracion_revoca_toda_la_familia_del_respaldo()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var sesion = await escenario.RegistrarAsync(EscenarioIdentidad.CorreoNuevo());
        var id = EscenarioIdentidad.UsuarioId(sesion);
        var ahora = escenario.Reloj.GetUtcNow();
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();

        // Act — mismo UPDATE del runbook, acotado a esta cuenta para no afectar pruebas concurrentes
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE identidad.sesion_refresh SET revocado_utc = date_trunc('hour', {ahora})
            WHERE usuario_id = {id} AND revocado_utc IS NULL
            """, EscenarioIdentidad.Ct);

        // Assert
        var renovar = await escenario.RenovarAsync(EscenarioIdentidad.TokenRenovacion(sesion));
        Assert.Equal(HttpStatusCode.Unauthorized, renovar.StatusCode);
    }
}
