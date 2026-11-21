using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Domain;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MotivoContrato = KillaUru.Anomalias.Contracts.MotivoDescarte;

using EstadoAnomalia = KillaUru.Anomalias.Domain.EstadoAnomalia;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// Parte 004 del escenario EQ-12 (integridad del dato); 005 debe repetirla por HTTP calculando el
/// contexto de recepción. Spec 004 · FR-003, FR-004, FR-011, FR-013.
/// </summary>
[Collection("Anomalias")]
public sealed class IntegridadDatoTests(BaseDatosPrueba bd)
{
    /// <summary>Cuatro vehículos anónimos (sin sesión) no bastan para publicar una grave (FR-011).</summary>
    [Fact]
    public async Task Anonimos_no_confirman_una_grave()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var inicial = escenario.Evento(5) with { DetectadoUtc = escenario.Reloj.GetUtcNow().AddMinutes(-15) };
        await escenario.ProcesarAsync(inicial);

        // Act — 4 vehículos más, separados 3 min, todos sin sesión
        for (var vehiculo = 1; vehiculo <= 4; vehiculo++)
        {
            var otro = inicial with { EventoId = Guid.NewGuid(), Seudonimo = Guid.NewGuid(), DetectadoUtc = inicial.DetectadoUtc!.Value.AddMinutes(vehiculo * 3) };
            Assert.Null((await escenario.ProcesarAsync(otro)).Motivo);
        }

        // Assert
        var anomalia = await escenario.LeerAnomaliaAsync(await escenario.AnomaliaDelEventoAsync(inicial.EventoId));
        Assert.Equal(EstadoAnomalia.PendienteConfirmacion, anomalia.Estado);
    }

    /// <summary>A unos 1 100 m de la vía el evento se descarta como FueraDelCorredor (FR-003).</summary>
    [Fact]
    public async Task Evento_lejos_de_la_via_se_descarta()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);

        // Act
        var resultado = await escenario.ProcesarAsync(escenario.Evento() with { Latitud = -12.99 });

        // Assert
        Assert.Equal(MotivoContrato.FueraDelCorredor, resultado.Motivo);
    }

    /// <summary>Saltar 0,1° (unos 11 km) en 10 s es imposible: se descarta por velocidad (FR-004).</summary>
    [Fact]
    public async Task Salto_imposible_del_mismo_seudonimo_se_descarta()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var inicial = escenario.Evento();
        await escenario.ProcesarAsync(inicial);
        var salto = inicial with { EventoId = Guid.NewGuid(), Longitud = escenario.Longitud + 0.1, DetectadoUtc = inicial.DetectadoUtc!.Value.AddSeconds(10) };

        // Act
        var resultado = await escenario.ProcesarAsync(salto);

        // Assert
        Assert.Equal(MotivoContrato.VelocidadImposible, resultado.Motivo);
    }

    /// <summary>
    /// Un celular sin conexión sube 5 h de detecciones (300, una por minuto) de golpe: ninguna se
    /// pierde por llegar tarde ni por el tope, porque el tope cuenta por hora de detección (FR-013).
    /// </summary>
    [Fact]
    public async Task Subida_de_cinco_horas_conserva_todos_los_reportes()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var modelo = "integridad-" + Guid.NewGuid();

        // Act
        await ProcesarRafagaAsync(escenario, modelo, 300, i => escenario.Reloj.GetUtcNow().AddHours(-5).AddMinutes(i), i => i % 60 + 1);

        // Assert
        Assert.Equal(300, await ContarReportesAsync(escenario, modelo));
    }

    /// <summary>
    /// Una lluvia de 600 eventos en una hora (más que el tope de 120) se guarda entera, pero la
    /// anomalía nueva que crea queda pendiente de otro vehículo (FR-013).
    /// </summary>
    [Fact]
    public async Task Lluvia_de_600_eventos_se_guarda_y_queda_pendiente()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var modelo = "integridad-" + Guid.NewGuid();

        // Act — a partir del evento 120 la lluvia se mueve 200 m: ahí nace una anomalía nueva ya con el tope superado
        var ultimo = await ProcesarRafagaAsync(escenario, modelo, 600, i => escenario.Reloj.GetUtcNow().AddHours(-1).AddSeconds(i * 6), i => i + 1,
            i => i >= 120 ? escenario.Longitud + 0.002 : escenario.Longitud);

        // Assert
        Assert.Equal(600, await ContarReportesAsync(escenario, modelo));
        var anomalia = await escenario.LeerAnomaliaAsync(await escenario.AnomaliaDelEventoAsync(ultimo));
        Assert.Equal(EstadoAnomalia.PendienteConfirmacion, anomalia.Estado);
    }

    /// <summary>Procesa eventos de un mismo dispositivo y devuelve el id del último.</summary>
    private static async Task<Guid> ProcesarRafagaAsync(EscenarioAnomalias escenario, string modelo, int cantidad,
        Func<int, DateTimeOffset> detectado, Func<int, int> conteoEnLaHora, Func<int, double>? longitud = null)
    {
        var dispositivo = Guid.NewGuid();
        var ultimo = Guid.Empty;
        for (var i = 0; i < cantidad; i++)
        {
            var evento = escenario.Evento() with
            {
                Seudonimo = dispositivo, ModeloVersion = modelo, DetectadoUtc = detectado(i), Longitud = longitud?.Invoke(i) ?? escenario.Longitud,
            };
            Assert.Null((await escenario.ProcesarAsync(evento, conteo: conteoEnLaHora(i))).Motivo);
            ultimo = evento.EventoId;
        }
        return ultimo;
    }

    private static async Task<int> ContarReportesAsync(EscenarioAnomalias escenario, string modelo)
    {
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>().Reportes.CountAsync(r => r.ModeloVersion == modelo, EscenarioAnomalias.Ct);
    }
}
