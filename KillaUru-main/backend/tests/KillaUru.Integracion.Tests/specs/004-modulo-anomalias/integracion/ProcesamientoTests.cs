using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Domain;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

using EstadoAnomalia = KillaUru.Anomalias.Domain.EstadoAnomalia;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// Procesamiento contra PostgreSQL/PostGIS real: idempotencia por eventoId, agrupación bajo bloqueo
/// y confirmación de una anomalía retenida por tope. Spec 004 · FR-006, FR-007, FR-008, FR-011, FR-013, FR-014, FR-031.
/// </summary>
[Collection("Anomalias")]
public sealed class ProcesamientoTests(BaseDatosPrueba bd)
{
    /// <summary>
    /// El mismo evento procesado 8 veces a la vez se guarda una sola vez; los otros 7 reciben
    /// «ya procesado» y el mapa recibe un único aviso, después del commit (FR-006, FR-014).
    /// </summary>
    [Fact]
    public async Task Un_evento_en_paralelo_no_duplica_y_publica_una_vez()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var evento = escenario.Evento();

        // Act
        var resultados = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => escenario.ProcesarAsync(evento)));

        // Assert
        Assert.Equal(7, resultados.Count(r => r.YaProcesado));
        Assert.All(resultados, r => Assert.Null(r.Motivo));
        var anomalia = await escenario.LeerAnomaliaAsync(await escenario.AnomaliaDelEventoAsync(evento.EventoId));
        Assert.Equal(EstadoAnomalia.Activa, anomalia.Estado);
        await escenario.Publicador.Received(1).PublicarAsync(Arg.Is<AnomaliaCambiada>(c => c.AnomaliaId == anomalia.Id), Arg.Any<CancellationToken>());
    }

    /// <summary>20 vehículos que reportan el mismo punto a la vez forman una sola anomalía (FR-007, FR-008).</summary>
    [Fact]
    public async Task Veinte_reportes_simultaneos_del_mismo_punto_forman_una_anomalia()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var eventos = Enumerable.Range(0, 20).Select(_ => escenario.Evento()).ToArray();

        // Act
        var resultados = await Task.WhenAll(eventos.Select(evento => escenario.ProcesarAsync(evento)));

        // Assert
        Assert.All(resultados, r => Assert.Null(r.Motivo));
        var anomalias = await Task.WhenAll(eventos.Select(e => escenario.AnomaliaDelEventoAsync(e.EventoId)));
        Assert.Single(anomalias.Distinct());
    }

    /// <summary>
    /// Una leve creada por tope queda pendiente; un segundo vehículo a 3 min la confirma sin necesitar
    /// sesión porque no es grave. La métrica del tope sube una vez y nunca se emite riesgo (FR-011, FR-013, FR-031).
    /// </summary>
    [Fact]
    public async Task Tope_retiene_y_otro_vehiculo_confirma_sin_sesion_si_no_es_grave()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var primero = escenario.Evento();
        Assert.Null((await escenario.ProcesarAsync(primero, conteo: 121)).Motivo);
        escenario.Reloj.Advance(TimeSpan.FromMinutes(3));

        // Act
        var segundo = await escenario.ProcesarAsync(escenario.Evento());

        // Assert
        Assert.Null(segundo.Motivo);
        var id = await escenario.AnomaliaDelEventoAsync(primero.EventoId);
        Assert.Equal(EstadoAnomalia.Activa, (await escenario.LeerAnomaliaAsync(id)).Estado);
        Assert.Equal(2, await ContarTransicionesAsync(escenario, id));
        escenario.Metricas.Received(1).RegistrarRetenidoPorTope();
        escenario.Metricas.DidNotReceive().RegistrarAccionRiesgo(Arg.Any<string>());
    }

    private static async Task<int> ContarTransicionesAsync(EscenarioAnomalias escenario, Guid anomaliaId)
    {
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>().Transiciones.CountAsync(t => t.AnomaliaId == anomaliaId, EscenarioAnomalias.Ct);
    }
}
