using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Domain;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Eventos;
using KillaUru.Shared.Metricas;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// PostGIS y transacciones reales; reloj, eventos y métricas sustituidos. Cada escenario usa su
/// propia longitud sobre la vía sintética para no agruparse con anomalías de otras pruebas.
/// </summary>
internal sealed class EscenarioAnomalias : IDisposable
{
    private static int secuencia;

    internal FakeTimeProvider Reloj { get; } = new(new DateTimeOffset(2026, 10, 24, 12, 0, 0, TimeSpan.Zero));
    internal IPublicadorEventos Publicador { get; } = Substitute.For<IPublicadorEventos>();
    internal IMetricas Metricas { get; } = Substitute.For<IMetricas>();
    internal WebApplicationFactory<Program> Api { get; }
    internal double Longitud { get; } = -76 + Interlocked.Increment(ref secuencia) * 0.003;
    internal static CancellationToken Ct => TestContext.Current.CancellationToken;

    internal EscenarioAnomalias(BaseDatosPrueba bd)
    {
        Api = new FabricaApi(bd).WithWebHostBuilder(b => b.ConfigureTestServices(s =>
        {
            s.AddSingleton<TimeProvider>(Reloj);
            s.AddSingleton(Publicador);
            s.AddSingleton(Metricas);
        }));
    }

    /// <summary>Evento válido sobre la vía, de un vehículo nuevo, detectado ahora.</summary>
    internal EventoReportado Evento(int severidad = 2) =>
        new(Guid.NewGuid(), Reloj.GetUtcNow(), -13, Longitud, 2, "Bache", severidad, 0.9, "prueba", Guid.NewGuid());

    /// <summary>Procesa el evento como lo haría Ingesta, con el contexto de recepción indicado.</summary>
    internal async Task<ResultadoProceso> ProcesarAsync(EventoReportado evento, bool sesion = false, int conteo = 1)
    {
        await using var scope = Api.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IAnomaliasService>()
            .ProcesarReporteAsync(evento, new(sesion, conteo, Reloj.GetUtcNow()), Ct);
    }

    /// <summary>Procesa el evento y devuelve el id de la anomalía a la que quedó asignado.</summary>
    internal async Task<Guid> ProcesarYObtenerAnomaliaAsync(EventoReportado evento)
    {
        await ProcesarAsync(evento);
        return await AnomaliaDelEventoAsync(evento.EventoId);
    }

    internal async Task<Guid> AnomaliaDelEventoAsync(Guid eventoId)
    {
        await using var scope = Api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        return (await db.Reportes.AsNoTracking().SingleAsync(r => r.EventoId == eventoId, Ct)).AnomaliaId;
    }

    /// <summary>Lee la anomalía tal como quedó en la base (sin la caché de EF).</summary>
    internal async Task<Anomalia> LeerAnomaliaAsync(Guid id)
    {
        await using var scope = Api.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>().Anomalias.AsNoTracking().SingleAsync(a => a.Id == id, Ct);
    }

    internal async Task<ReporteAnomalia> LeerReporteAsync(Guid eventoId)
    {
        await using var scope = Api.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>().Reportes.AsNoTracking().SingleAsync(r => r.EventoId == eventoId, Ct);
    }

    /// <summary>Lleva la base a un estado que el flujo normal tardaría días en producir.</summary>
    internal async Task EjecutarSqlAsync(FormattableString sql)
    {
        await using var scope = Api.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>().Database.ExecuteSqlInterpolatedAsync(sql, Ct);
    }

    /// <summary>Cliente HTTP con JWT del rol indicado, emitido con el reloj del escenario.</summary>
    internal HttpClient ClienteComo(string rol, Guid? actor = null)
    {
        var cliente = Api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", Tokens.Emitir(rol, actor?.ToString(), reloj: Reloj));
        return cliente;
    }

    public void Dispose() => Api.Dispose();
}
