using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>Parte 004 de EQ-12; 005 debe repetirla por HTTP calculando el contexto de recepción.</summary>
[Collection("Anomalias")]
public sealed class IntegridadDatoTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Anonimos_no_confirman_graves_y_se_rechazan_fuera_de_via_y_saltos()
    {
        using var e=new EscenarioAnomalias(bd); var inicial=e.Evento(5) with { DetectadoUtc=e.Reloj.GetUtcNow().AddMinutes(-15) };
        await e.ProcesarAsync(inicial);
        for(var i=1;i<=4;i++) Assert.Null((await e.ProcesarAsync(inicial with { EventoId=Guid.NewGuid(), Seudonimo=Guid.NewGuid(), DetectadoUtc=inicial.DetectadoUtc!.Value.AddMinutes(i*3) })).Motivo);
        Assert.Equal(MotivoDescarte.FueraDelCorredor,(await e.ProcesarAsync(e.Evento() with { Latitud=-12.99 })).Motivo);
        var salto=inicial with { EventoId=Guid.NewGuid(), Longitud=e.Longitud+.1, DetectadoUtc=inicial.DetectadoUtc!.Value.AddSeconds(10) };
        Assert.Equal(MotivoDescarte.VelocidadImposible,(await e.ProcesarAsync(salto)).Motivo);
        await using var scope=e.Api.Services.CreateAsyncScope();var ct=TestContext.Current.CancellationToken;
        var db=scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();var id=(await db.Reportes.SingleAsync(r=>r.EventoId==inicial.EventoId,ct)).AnomaliaId;
        Assert.Equal("PendienteConfirmacion",(await db.Anomalias.SingleAsync(a=>a.Id==id,ct)).Estado.ToString());
    }
    [Theory]
    [InlineData(false,300)]
    [InlineData(true,600)]
    public async Task Subida_de_cinco_horas_y_lluvia_conservan_todos_los_reportes(bool lluvia,int cantidad)
    {
        using var e=new EscenarioAnomalias(bd);var dispositivo=Guid.NewGuid();var modelo="integridad-"+Guid.NewGuid();
        var ahora=e.Reloj.GetUtcNow();Guid ultimo=Guid.Empty;
        for(var i=0;i<cantidad;i++)
        {
            var detectado=lluvia?ahora.AddHours(-1).AddSeconds(i*6):ahora.AddHours(-5).AddMinutes(i);
            var evento=e.Evento() with { Seudonimo=dispositivo,ModeloVersion=modelo,DetectadoUtc=detectado,
                Longitud=lluvia&&i>=120?e.Longitud+.002:e.Longitud };
            var resultado=await e.ProcesarAsync(evento,conteo:lluvia?i+1:i%60+1);Assert.Null(resultado.Motivo);ultimo=evento.EventoId;
        }
        await using var scope=e.Api.Services.CreateAsyncScope();var ct=TestContext.Current.CancellationToken;
        var db=scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        Assert.Equal(cantidad,await db.Reportes.CountAsync(r=>r.ModeloVersion==modelo,ct));
        if(lluvia)
        {
            var id=(await db.Reportes.SingleAsync(r=>r.EventoId==ultimo,ct)).AnomaliaId;
            Assert.Equal("PendienteConfirmacion",(await db.Anomalias.SingleAsync(a=>a.Id==id,ct)).Estado.ToString());
        }
    }
}
