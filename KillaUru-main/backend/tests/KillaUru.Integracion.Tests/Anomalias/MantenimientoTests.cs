using KillaUru.Anomalias.Application.Tareas;
using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Contracts.Lecturas;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EstadoDominio = KillaUru.Anomalias.Domain.EstadoAnomalia;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// Tareas de mantenimiento en PostgreSQL con reloj controlado: caducidad, purga de seudónimos y
/// lecturas que otros módulos usan después. Spec 004 · FR-018, FR-021, FR-029.
/// </summary>
[Collection("Anomalias")]
public sealed class MantenimientoTests(BaseDatosPrueba bd)
{
    /// <summary>
    /// Caducar fecha la expiración en el vencimiento (no en la hora de la tarea), deja una sola
    /// transición y aparece en «cambios desde» para que el mapa se actualice (FR-018, FR-029).
    /// </summary>
    [Fact]
    public async Task Caducidad_conserva_el_instante_de_vencimiento_y_la_lectura_incremental()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var id = await escenario.ProcesarYObtenerAnomaliaAsync(escenario.Evento());
        var antes = await escenario.LeerAnomaliaAsync(id);
        escenario.Reloj.Advance(TimeSpan.FromHours(73));
        await using var scope = escenario.Api.Services.CreateAsyncScope();

        // Act
        await scope.ServiceProvider.GetRequiredService<CaducarAnomaliasUseCase>().EjecutarAsync(EscenarioAnomalias.Ct);

        // Assert
        var despues = await escenario.LeerAnomaliaAsync(id);
        Assert.Equal(EstadoDominio.Expirada, despues.Estado);
        Assert.Equal(antes.VenceUtc, despues.EstadoDesdeUtc);
        Assert.Equal(1, await ExpiracionesAsync(scope.ServiceProvider, id));
        var cambios = await scope.ServiceProvider.GetRequiredService<IAnomaliaLectura>().CambiosDesdeAsync(antes.ActualizadaUtc, EscenarioAnomalias.Ct);
        Assert.Contains(cambios, a => a.Id == id && a.Estado == EstadoAnomalia.Expirada);
    }

    /// <summary>El filtro por caja geográfica y severidad usa la geometría real de PostGIS (FR-029).</summary>
    [Fact]
    public async Task Filtrado_usa_la_geometria()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var id = await escenario.ProcesarYObtenerAnomaliaAsync(escenario.Evento());
        var caja = new FiltroAnomalias(escenario.Longitud - 0.001, -13.001, escenario.Longitud + 0.001, -12.999, Severidades: [2]);
        await using var scope = escenario.Api.Services.CreateAsyncScope();

        // Act
        var encontradas = await scope.ServiceProvider.GetRequiredService<IAnomaliaLectura>().FiltrarAsync(caja, EscenarioAnomalias.Ct);

        // Assert
        Assert.Contains(encontradas, a => a.Id == id);
    }

    /// <summary>
    /// Pasadas 72 h, la purga borra el seudónimo y redondea la recepción a la hora en la misma
    /// sentencia, sin cambiar el conteo diario del dashboard (FR-021).
    /// </summary>
    [Fact]
    public async Task Purga_anonimiza_sin_cambiar_el_conteo_diario()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var evento = await RecibirConMinutosYSegundosAsync(escenario);
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var lectura = scope.ServiceProvider.GetRequiredService<IAnomaliaLectura>();
        var dia = DateOnly.FromDateTime(escenario.Reloj.GetUtcNow().ToOffset(TimeSpan.FromHours(-5)).DateTime);
        var conteoAntes = (await lectura.ResumenDelDiaAsync(dia, EscenarioAnomalias.Ct)).EventosRecibidos;
        escenario.Reloj.Advance(TimeSpan.FromHours(72) + TimeSpan.FromSeconds(1));

        // Act
        await scope.ServiceProvider.GetRequiredService<PurgarSeudonimosUseCase>().EjecutarAsync(EscenarioAnomalias.Ct);

        // Assert
        var reporte = await escenario.LeerReporteAsync(evento.EventoId);
        Assert.Null(reporte.Seudonimo);
        Assert.Equal(0, reporte.RecibidoUtc.Minute);
        Assert.Equal(conteoAntes, (await lectura.ResumenDelDiaAsync(dia, EscenarioAnomalias.Ct)).EventosRecibidos);
    }

    /// <summary>Recepción a las 12:37:12: con minutos y segundos, para que se note el redondeo a la hora.</summary>
    private static async Task<EventoReportado> RecibirConMinutosYSegundosAsync(EscenarioAnomalias escenario)
    {
        escenario.Reloj.Advance(TimeSpan.FromMinutes(37) + TimeSpan.FromSeconds(12));
        var evento = escenario.Evento();
        await escenario.ProcesarAsync(evento);
        return evento;
    }

    private static Task<int> ExpiracionesAsync(IServiceProvider servicios, Guid id) =>
        servicios.GetRequiredService<AnomaliasDbContext>().Transiciones
            .CountAsync(t => t.AnomaliaId == id && t.Hacia == EstadoDominio.Expirada, EscenarioAnomalias.Ct);
}
