using KillaUru.Anomalias.Application.Administracion;
using KillaUru.Anomalias.Application.Tareas;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>Fronteras de rejilla y carrera entre administrador y caducidad, con conexiones independientes.</summary>
[Collection("Anomalias")]
public sealed class ConcurrenciaAdicionalTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Ocho_trabajadores_a_ambos_lados_de_cuatro_celdas_agrupan_sin_interbloqueo()
    {
        using var e=new EscenarioAnomalias(bd);var centro=Math.Round(e.Longitud/.005)*.005;
        var eventos=Enumerable.Range(0,8).Select(i=>e.Evento() with { Latitud=-13+(i%2==0?-.00005:.00005),Longitud=centro+(i%4<2?-.00005:.00005) }).ToArray();
        var resultados=await Task.WhenAll(eventos.Select(v=>e.ProcesarAsync(v))).WaitAsync(TimeSpan.FromSeconds(20),TestContext.Current.CancellationToken);
        Assert.All(resultados,r=>Assert.Null(r.Motivo));
        await using var scope=e.Api.Services.CreateAsyncScope();var ids=eventos.Select(v=>v.EventoId).ToArray();
        var db=scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        var grupos=await db.Reportes.Where(r=>ids.Contains(r.EventoId)).Select(r=>r.AnomaliaId).Distinct().ToArrayAsync(TestContext.Current.CancellationToken);
        Assert.Single(grupos);
    }
    [Fact]
    public async Task Moderacion_y_caducidad_concurrentes_no_pierden_estado_ni_historial()
    {
        using var e=new EscenarioAnomalias(bd);var evento=e.Evento();await e.ProcesarAsync(evento);
        await using var lectura=e.Api.Services.CreateAsyncScope();var ct=TestContext.Current.CancellationToken;
        var db=lectura.ServiceProvider.GetRequiredService<AnomaliasDbContext>();var id=(await db.Reportes.SingleAsync(r=>r.EventoId==evento.EventoId,ct)).AnomaliaId;
        e.Reloj.Advance(TimeSpan.FromHours(73));
        await using var s1=e.Api.Services.CreateAsyncScope();await using var s2=e.Api.Services.CreateAsyncScope();
        var moderar=s1.ServiceProvider.GetRequiredService<ModerarAnomaliaUseCase>().EjecutarAsync(id,new(AccionModeracion.MarcarReparada),new(Guid.NewGuid(),"Administrador"),ct);
        var caducar=s2.ServiceProvider.GetRequiredService<CaducarAnomaliasUseCase>().EjecutarAsync(ct);
        await Task.WhenAll(moderar,caducar).WaitAsync(TimeSpan.FromSeconds(20),ct);
        var a=await db.Anomalias.AsNoTracking().SingleAsync(a=>a.Id==id,ct);
        Assert.True(a.Estado is KillaUru.Anomalias.Domain.EstadoAnomalia.Expirada or KillaUru.Anomalias.Domain.EstadoAnomalia.Reparada);
        var historia=await db.Transiciones.Where(t=>t.AnomaliaId==id).OrderBy(t=>t.Id).ToListAsync(ct);
        Assert.Equal(2,historia.Count);Assert.Equal(a.Estado,historia[^1].Hacia);
        Assert.Equal(a.EstadoDesdeUtc,historia[^1].OcurridaUtc);
    }
}
