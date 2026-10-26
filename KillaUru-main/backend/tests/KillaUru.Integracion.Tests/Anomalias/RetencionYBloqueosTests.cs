using KillaUru.Anomalias.Application.Tareas;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>Fronteras estrictas de retención y bloqueo real de una fila en procesamiento.</summary>
[Collection("Anomalias")]
public sealed class RetencionYBloqueosTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Caducidad_salta_una_fila_bloqueada_y_la_recupera_en_la_siguiente_pasada()
    {
        using var e = new EscenarioAnomalias(bd); var evento=e.Evento(); await e.ProcesarAsync(evento);
        await using var bloqueador=e.Api.Services.CreateAsyncScope(); var ct=TestContext.Current.CancellationToken;
        var db=bloqueador.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        var id=(await db.Reportes.SingleAsync(r=>r.EventoId==evento.EventoId,ct)).AnomaliaId;
        e.Reloj.Advance(TimeSpan.FromHours(73));
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM anomalias.anomalia WHERE id={id} FOR UPDATE",ct);
        await using var trabajador=e.Api.Services.CreateAsyncScope();
        var tarea=trabajador.ServiceProvider.GetRequiredService<CaducarAnomaliasUseCase>();
        await tarea.EjecutarAsync(ct).WaitAsync(TimeSpan.FromSeconds(10),ct);
        Assert.Equal("Activa",(await db.Anomalias.AsNoTracking().SingleAsync(a=>a.Id==id,ct)).Estado.ToString());
        await tx.RollbackAsync(ct);
        await tarea.EjecutarAsync(ct);
        Assert.Equal("Expirada",(await db.Anomalias.AsNoTracking().SingleAsync(a=>a.Id==id,ct)).Estado.ToString());
    }
    [Theory]
    [InlineData(3,0,800,false)]
    [InlineData(3,1,800,true)]
    [InlineData(4,1,800,true)]
    [InlineData(5,1,365,false)]
    [InlineData(5,1,800,true)]
    public async Task Historico_respeta_limite_y_actividad_de_descartadas(int estado,int segundosExtra,int diasUltima,bool eliminada)
    {
        using var e=new EscenarioAnomalias(bd); var evento=e.Evento(); await e.ProcesarAsync(evento);
        await using var scope=e.Api.Services.CreateAsyncScope(); var ct=TestContext.Current.CancellationToken;
        var db=scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        var id=(await db.Reportes.SingleAsync(r=>r.EventoId==evento.EventoId,ct)).AnomaliaId;
        var desde=e.Reloj.GetUtcNow().AddDays(-730).AddSeconds(-segundosExtra); var ultima=e.Reloj.GetUtcNow().AddDays(-diasUltima);
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE anomalias.anomalia SET estado={(short)estado}, estado_desde_utc={desde}, ultima_deteccion_utc={ultima} WHERE id={id}",ct);
        await scope.ServiceProvider.GetRequiredService<PurgarHistoricoUseCase>().EjecutarAsync(ct);
        Assert.Equal(!eliminada,await db.Anomalias.AnyAsync(a=>a.Id==id,ct));
    }
    [Fact]
    public async Task Deteccion_antigua_recibida_recientemente_conserva_seudonimo_y_hora()
    {
        using var e=new EscenarioAnomalias(bd); var evento=e.Evento() with { DetectadoUtc=e.Reloj.GetUtcNow().AddDays(-6) };
        await e.ProcesarAsync(evento); e.Reloj.Advance(TimeSpan.FromHours(1));
        await using var scope=e.Api.Services.CreateAsyncScope(); var ct=TestContext.Current.CancellationToken;
        await scope.ServiceProvider.GetRequiredService<PurgarSeudonimosUseCase>().EjecutarAsync(ct);
        var r=await scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>().Reportes.SingleAsync(r=>r.EventoId==evento.EventoId,ct);
        Assert.Equal(evento.Seudonimo,r.Seudonimo); Assert.Equal(e.Reloj.GetUtcNow().AddHours(-1),r.RecibidoUtc);
    }
}
