using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Contracts.Lecturas;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// Lecturas que consumen Mapa y Dashboard: el resumen diario cuenta por fecha de Lima y clasifica
/// cada transición según el contrato. Spec 004 · FR-029, FR-032.
/// </summary>
[Collection("Anomalias")]
public sealed class LecturasTests(BaseDatosPrueba bd)
{
    /// <summary>04:30 UTC del 2 de enero es todavía el 1 de enero en Lima.</summary>
    private static readonly DateTimeOffset MadrugadaUtc = new(2030, 1, 2, 4, 30, 0, TimeSpan.Zero);

    /// <summary>
    /// Inserta una transición de cada clase: renacimientos (desde Expirada), confirmación (Pendiente →
    /// Activa), pendiente expirada y expiraciones.
    /// </summary>
    private static Task SembrarTransicionesAsync(EscenarioAnomalias escenario, Guid id, DateTimeOffset instante) =>
        escenario.EjecutarSqlAsync($"""
            INSERT INTO anomalias.transicion(anomalia_id,desde,hacia,ocurrida_utc) VALUES
            ({id},3,1,{instante}),({id},3,2,{instante}),({id},4,1,{instante}),
            ({id},2,1,{instante}),({id},2,3,{instante}),({id},1,3,{instante})
            """);

    private static async Task<ResumenDia> ResumenAsync(EscenarioAnomalias escenario, DateOnly diaLima)
    {
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IAnomaliaLectura>().ResumenDelDiaAsync(diaLima, EscenarioAnomalias.Ct);
    }

    /// <summary>Evento a las 23:30 de Lima (1 ene) con todas las transiciones, y otro a las 00:30 (2 ene).</summary>
    private static async Task UnEventoACadaLadoDeLaMedianocheAsync(EscenarioAnomalias escenario)
    {
        escenario.Reloj.SetUtcNow(MadrugadaUtc);
        var id = await escenario.ProcesarYObtenerAnomaliaAsync(escenario.Evento());
        escenario.Reloj.Advance(TimeSpan.FromHours(1));
        await escenario.ProcesarAsync(escenario.Evento());
        await SembrarTransicionesAsync(escenario, id, MadrugadaUtc);
    }

    /// <summary>
    /// Un evento a las 23:30 de Lima cuenta el 1 de enero y otro una hora después el 2. Reparada →
    /// Activa no es renacimiento: solo cuentan los que vienen de Expirada (FR-029).
    /// </summary>
    [Fact]
    public async Task Resumen_del_dia_usa_la_fecha_de_Lima_y_la_semantica_del_contrato()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        await UnEventoACadaLadoDeLaMedianocheAsync(escenario);

        // Act
        var primeroDeEnero = await ResumenAsync(escenario, new DateOnly(2030, 1, 1));
        var dosDeEnero = await ResumenAsync(escenario, new DateOnly(2030, 1, 2));

        // Assert
        Assert.Equal(1, primeroDeEnero.EventosRecibidos);
        Assert.Equal(1, primeroDeEnero.DispositivosDistintos);
        Assert.Equal(2, primeroDeEnero.Renacimientos);
        Assert.Equal(1, primeroDeEnero.Confirmaciones);
        Assert.Equal(1, primeroDeEnero.PendientesExpiradas);
        Assert.Equal(2, primeroDeEnero.Expiraciones);
        Assert.Equal(1, dosDeEnero.EventosRecibidos);
    }

    /// <summary>
    /// Las activas por celda incluyen el km sobre la vía (la app lo usa para las alertas) y «cambios
    /// desde ahora» no devuelve lo que no cambió (FR-029, FR-032).
    /// </summary>
    [Fact]
    public async Task Activas_por_celda_traen_el_km_y_cambios_desde_es_incremental()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var id = await escenario.ProcesarYObtenerAnomaliaAsync(escenario.Evento());
        var anomalia = await escenario.LeerAnomaliaAsync(id);
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var lectura = scope.ServiceProvider.GetRequiredService<IAnomaliaLectura>();

        // Act
        var activas = await lectura.ActivasPorCeldasAsync([anomalia.Celda.Id], EscenarioAnomalias.Ct);
        var cambiosDesdeAhora = await lectura.CambiosDesdeAsync(escenario.Reloj.GetUtcNow().AddSeconds(1), EscenarioAnomalias.Ct);

        // Assert
        Assert.Contains(activas, v => v.Id == id && v.KmVia == anomalia.KmVia);
        Assert.DoesNotContain(cambiosDesdeAhora, v => v.Id == id);
    }
}
