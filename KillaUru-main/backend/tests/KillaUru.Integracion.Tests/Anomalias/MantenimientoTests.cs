using KillaUru.Anomalias.Application.Tareas;
using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Contracts.Lecturas;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>Verifica estados y retención en PostgreSQL con reloj controlado.</summary>
[Collection("Anomalias")]
public sealed class MantenimientoTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Caducidad_conserva_instante_de_vencimiento_y_lectura_incremental()
    {
        using var e = new EscenarioAnomalias(bd); var evento = e.Evento(); await e.ProcesarAsync(evento);
        await using var scope = e.Api.Services.CreateAsyncScope(); var ct = TestContext.Current.CancellationToken;
        var db = scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        var r = await db.Reportes.SingleAsync(r => r.EventoId == evento.EventoId, ct);
        var antes = await db.Anomalias.AsNoTracking().SingleAsync(a => a.Id == r.AnomaliaId, ct);
        e.Reloj.Advance(TimeSpan.FromHours(73));
        await scope.ServiceProvider.GetRequiredService<CaducarAnomaliasUseCase>().EjecutarAsync(ct);
        var actual = await db.Anomalias.AsNoTracking().SingleAsync(a => a.Id == r.AnomaliaId, ct);
        Assert.Equal("Expirada", actual.Estado.ToString()); Assert.Equal(antes.VenceUtc, actual.EstadoDesdeUtc);
        Assert.Single(await db.Transiciones.Where(t => t.AnomaliaId == actual.Id && t.Hacia == KillaUru.Anomalias.Domain.EstadoAnomalia.Expirada).ToListAsync(ct));
        var lectura = scope.ServiceProvider.GetRequiredService<IAnomaliaLectura>();
        Assert.Contains(await lectura.CambiosDesdeAsync(antes.ActualizadaUtc, ct), a => a.Id == actual.Id && a.Estado == EstadoAnomalia.Expirada);
    }

    [Fact]
    public async Task Purga_conserva_conteo_diario_y_filtrado_usa_geometria()
    {
        using var e = new EscenarioAnomalias(bd); e.Reloj.Advance(TimeSpan.FromMinutes(37) + TimeSpan.FromSeconds(12));
        var evento = e.Evento(); await e.ProcesarAsync(evento);
        await using var scope = e.Api.Services.CreateAsyncScope(); var ct = TestContext.Current.CancellationToken;
        var lectura = scope.ServiceProvider.GetRequiredService<IAnomaliaLectura>();
        var db = scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        var r = await db.Reportes.AsNoTracking().SingleAsync(r => r.EventoId == evento.EventoId, ct);
        var filtro = new FiltroAnomalias(e.Longitud - .001, -13.001, e.Longitud + .001, -12.999, Severidades: [2]);
        Assert.Contains(await lectura.FiltrarAsync(filtro, ct), a => a.Id == r.AnomaliaId);
        var dia = DateOnly.FromDateTime(r.RecibidoUtc.ToOffset(TimeSpan.FromHours(-5)).DateTime);
        var antes = await lectura.ResumenDelDiaAsync(dia, ct);
        e.Reloj.Advance(TimeSpan.FromHours(72) + TimeSpan.FromSeconds(1));
        await scope.ServiceProvider.GetRequiredService<PurgarSeudonimosUseCase>().EjecutarAsync(ct);
        var purgado = await db.Reportes.AsNoTracking().SingleAsync(r => r.EventoId == evento.EventoId, ct);
        Assert.Null(purgado.Seudonimo); Assert.Equal(0, purgado.RecibidoUtc.Minute);
        Assert.Equal(antes.EventosRecibidos, (await lectura.ResumenDelDiaAsync(dia, ct)).EventosRecibidos);
    }
}
