using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>Conteos por fecha local y semántica de renacimiento, independiente de las reparaciones.</summary>
[Collection("Anomalias")]
public sealed class LecturasTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Dia_de_Lima_y_renacimientos_se_cuentan_segun_el_contrato()
    {
        using var e=new EscenarioAnomalias(bd); e.Reloj.SetUtcNow(new DateTimeOffset(2030,1,2,4,30,0,TimeSpan.Zero));
        var evento=e.Evento(); await e.ProcesarAsync(evento);var instante=e.Reloj.GetUtcNow();
        e.Reloj.Advance(TimeSpan.FromHours(1));await e.ProcesarAsync(e.Evento());
        await using var scope=e.Api.Services.CreateAsyncScope();var ct=TestContext.Current.CancellationToken;
        var db=scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();var id=(await db.Reportes.SingleAsync(r=>r.EventoId==evento.EventoId,ct)).AnomaliaId;
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO anomalias.transicion(anomalia_id,desde,hacia,ocurrida_utc) VALUES ({id},3,1,{instante}),({id},3,2,{instante}),({id},4,1,{instante}),({id},2,1,{instante}),({id},2,3,{instante}),({id},1,3,{instante})",ct);
        var lector=scope.ServiceProvider.GetRequiredService<IAnomaliaLectura>();
        var dia=await lector.ResumenDelDiaAsync(new DateOnly(2030,1,1),ct);
        Assert.Equal(1,dia.EventosRecibidos);Assert.Equal(1,dia.DispositivosDistintos);
        Assert.Equal(2,dia.Renacimientos);Assert.Equal(1,dia.Confirmaciones);Assert.Equal(1,dia.PendientesExpiradas);Assert.Equal(2,dia.Expiraciones);
        Assert.Equal(1,(await lector.ResumenDelDiaAsync(new DateOnly(2030,1,2),ct)).EventosRecibidos);
        var a=await db.Anomalias.SingleAsync(a=>a.Id==id,ct);
        Assert.Contains(await lector.ActivasPorCeldasAsync([a.Celda.Id],ct),v=>v.Id==id&&v.KmVia==a.KmVia);
        Assert.DoesNotContain(await lector.CambiosDesdeAsync(e.Reloj.GetUtcNow(),ct),v=>v.Id==id);
    }
}
