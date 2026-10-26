using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>La idempotencia y los bloqueos se comprueban contra PostgreSQL/PostGIS real.</summary>
[Collection("Anomalias")]
public sealed class ProcesamientoTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Un_evento_en_paralelo_no_duplica_y_publica_despues_de_confirmar()
    {
        using var e = new EscenarioAnomalias(bd);
        var evento = e.Evento();
        var resultados = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => e.ProcesarAsync(evento)));
        Assert.Equal(7, resultados.Count(r => r.YaProcesado));
        Assert.All(resultados, r => Assert.Null(r.Motivo));
        await using var scope = e.Api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        Assert.Equal(1, await db.Reportes.CountAsync(r => r.EventoId == evento.EventoId, TestContext.Current.CancellationToken));
        var reporte = await db.Reportes.SingleAsync(r => r.EventoId == evento.EventoId, TestContext.Current.CancellationToken);
        var a = await db.Anomalias.SingleAsync(x => x.Id == reporte.AnomaliaId, TestContext.Current.CancellationToken);
        Assert.Equal("Activa", a.Estado.ToString());
        await e.Publicador.Received(1).PublicarAsync(Arg.Is<AnomaliaCambiada>(c => c.AnomaliaId == a.Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Veinte_reportes_simultaneos_del_mismo_punto_forman_una_anomalia()
    {
        using var e = new EscenarioAnomalias(bd);
        var eventos = Enumerable.Range(0, 20).Select(_ => e.Evento()).ToArray();
        var resultados = await Task.WhenAll(eventos.Select(x => e.ProcesarAsync(x)));
        Assert.All(resultados, r => Assert.Null(r.Motivo));
        await using var scope = e.Api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        var ids = eventos.Select(x => x.EventoId).ToArray();
        var anomalias = await db.Reportes.Where(r => ids.Contains(r.EventoId)).Select(r => r.AnomaliaId).Distinct().ToListAsync(TestContext.Current.CancellationToken);
        Assert.Single(anomalias);
    }

    [Fact]
    public async Task Tope_retiene_y_otro_vehiculo_confirma_sin_sesion_si_no_es_grave()
    {
        using var e = new EscenarioAnomalias(bd);
        var primero = e.Evento();
        Assert.Null((await e.ProcesarAsync(primero, conteo: 121)).Motivo);
        e.Reloj.Advance(TimeSpan.FromMinutes(3));
        Assert.Null((await e.ProcesarAsync(e.Evento())).Motivo);
        await using var scope = e.Api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        var id = await db.Reportes.Where(r => r.EventoId == primero.EventoId).Select(r => r.AnomaliaId).SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal("Activa", (await db.Anomalias.SingleAsync(a => a.Id == id, TestContext.Current.CancellationToken)).Estado.ToString());
        Assert.Equal(2, await db.Transiciones.CountAsync(t => t.AnomaliaId == id, TestContext.Current.CancellationToken));
        e.Metricas.Received(1).RegistrarRetenidoPorTope();
        e.Metricas.DidNotReceive().RegistrarAccionRiesgo(Arg.Any<string>());
    }
}
