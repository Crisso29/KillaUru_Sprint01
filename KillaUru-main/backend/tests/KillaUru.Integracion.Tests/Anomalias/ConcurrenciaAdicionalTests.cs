using KillaUru.Anomalias.Application.Administracion;
using KillaUru.Anomalias.Application.Tareas;
using KillaUru.Anomalias.Domain;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// Concurrencia en los bordes de la rejilla y carrera entre el administrador y la tarea de
/// caducidad, cada uno con su propia conexión. Spec 004 · FR-007, FR-015, FR-018, FR-030.
/// </summary>
[Collection("Anomalias")]
public sealed class ConcurrenciaAdicionalTests(BaseDatosPrueba bd)
{
    private static readonly TimeSpan SinInterbloqueo = TimeSpan.FromSeconds(20);

    /// <summary>
    /// 8 eventos repartidos en las 4 celdas que rodean una esquina se agrupan en una sola anomalía y
    /// sin interbloqueo, porque las celdas se bloquean siempre en orden ascendente (FR-007).
    /// </summary>
    [Fact]
    public async Task Ocho_trabajadores_en_cuatro_celdas_agrupan_sin_interbloqueo()
    {
        // Arrange — la esquina se alinea a la rejilla de 0,005° y cada evento cae 5 m hacia un lado
        using var escenario = new EscenarioAnomalias(bd);
        var esquina = Math.Round(escenario.Longitud / 0.005) * 0.005;
        var eventos = Enumerable.Range(0, 8).Select(i => escenario.Evento() with
        {
            Latitud = -13 + (i % 2 == 0 ? -0.00005 : 0.00005),
            Longitud = esquina + (i % 4 < 2 ? -0.00005 : 0.00005),
        }).ToArray();

        // Act
        var resultados = await Task.WhenAll(eventos.Select(e => escenario.ProcesarAsync(e))).WaitAsync(SinInterbloqueo, EscenarioAnomalias.Ct);

        // Assert
        Assert.All(resultados, r => Assert.Null(r.Motivo));
        var anomalias = await Task.WhenAll(eventos.Select(e => escenario.AnomaliaDelEventoAsync(e.EventoId)));
        Assert.Single(anomalias.Distinct());
    }

    /// <summary>
    /// Si el administrador repara mientras la tarea caduca, gana uno solo: el estado final y la última
    /// transición coinciden y no queda historia duplicada (FR-015, FR-030).
    /// </summary>
    [Fact]
    public async Task Moderacion_y_caducidad_concurrentes_no_pierden_estado_ni_historial()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var id = await escenario.ProcesarYObtenerAnomaliaAsync(escenario.Evento());
        escenario.Reloj.Advance(TimeSpan.FromHours(73));

        // Act
        await RepararMientrasCaducaAsync(escenario, id);

        // Assert
        var anomalia = await escenario.LeerAnomaliaAsync(id);
        var historia = await HistoriaAsync(escenario, id);
        Assert.True(anomalia.Estado is EstadoAnomalia.Expirada or EstadoAnomalia.Reparada);
        Assert.Equal(2, historia.Count);
        Assert.Equal(anomalia.Estado, historia[^1].Hacia);
        Assert.Equal(anomalia.EstadoDesdeUtc, historia[^1].OcurridaUtc);
    }

    /// <summary>Administrador y tarea de caducidad, cada uno en su propio scope (su propia conexión), a la vez.</summary>
    private static async Task RepararMientrasCaducaAsync(EscenarioAnomalias escenario, Guid id)
    {
        await using var administrador = escenario.Api.Services.CreateAsyncScope();
        await using var tarea = escenario.Api.Services.CreateAsyncScope();
        var moderar = administrador.ServiceProvider.GetRequiredService<ModerarAnomaliaUseCase>()
            .EjecutarAsync(id, new(AccionModeracion.MarcarReparada), new(Guid.NewGuid(), "Administrador"), EscenarioAnomalias.Ct);
        var caducar = tarea.ServiceProvider.GetRequiredService<CaducarAnomaliasUseCase>().EjecutarAsync(EscenarioAnomalias.Ct);
        await Task.WhenAll(moderar, caducar).WaitAsync(SinInterbloqueo, EscenarioAnomalias.Ct);
    }

    private static async Task<List<Transicion>> HistoriaAsync(EscenarioAnomalias escenario, Guid id)
    {
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>().Transiciones
            .Where(t => t.AnomaliaId == id).OrderBy(t => t.Id).ToListAsync(EscenarioAnomalias.Ct);
    }
}
