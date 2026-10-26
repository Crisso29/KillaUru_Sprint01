using System.Net;
using System.Net.Http.Json;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Auditoria.Contracts;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>Un fallo de Auditoría revierte la moderación y corta la exportación antes del archivo.</summary>
[Collection("Anomalias")]
public sealed class AtomicidadAuditoriaTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Fallo_de_auditoria_revierte_estado_e_historial()
    {
        using var e=new EscenarioAnomalias(bd); var evento=e.Evento(); await e.ProcesarAsync(evento);
        await using var scope=e.Api.Services.CreateAsyncScope(); var ct=TestContext.Current.CancellationToken;
        var db=scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        var id=(await db.Reportes.SingleAsync(r=>r.EventoId==evento.EventoId,ct)).AnomaliaId;
        var auditoria=Fallida(); using var api=e.Api.WithWebHostBuilder(b=>b.ConfigureTestServices(s=>s.AddSingleton(auditoria)));
        using var cliente=api.CreateClient(); cliente.DefaultRequestHeaders.Authorization=new("Bearer",Tokens.Emitir("Administrador",reloj:e.Reloj));
        using var respuesta=await cliente.PostAsJsonAsync($"/api/v1/anomalies/{id}/moderacion",new { accion="Descartar" },ct);
        Assert.Equal(HttpStatusCode.InternalServerError,respuesta.StatusCode);
        Assert.Equal("Activa",(await db.Anomalias.AsNoTracking().SingleAsync(a=>a.Id==id,ct)).Estado.ToString());
        Assert.Equal(1,await db.Transiciones.CountAsync(t=>t.AnomaliaId==id,ct));
    }
    [Fact]
    public async Task Fallo_de_auditoria_no_entrega_archivo()
    {
        using var e=new EscenarioAnomalias(bd); var auditoria=Fallida();
        using var api=e.Api.WithWebHostBuilder(b=>b.ConfigureTestServices(s=>s.AddSingleton(auditoria)));
        using var cliente=api.CreateClient(); cliente.DefaultRequestHeaders.Authorization=new("Bearer",Tokens.Emitir("Administrador",reloj:e.Reloj));
        using var respuesta=await cliente.PostAsJsonAsync("/api/v1/exportaciones",new { formato="CSV" },TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.InternalServerError,respuesta.StatusCode);
        Assert.Equal("application/problem+json",respuesta.Content.Headers.ContentType!.MediaType);
        Assert.DoesNotContain("primer_reporte",await respuesta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),StringComparison.Ordinal);
    }
    private static IAuditoriaService Fallida()
    {
        var a=Substitute.For<IAuditoriaService>();
        a.RegistrarAsync(Arg.Any<AccionAuditada>(),Arg.Any<ActorAuditoria>(),Arg.Any<string>(),Arg.Any<IReadOnlyDictionary<string,object?>>(),Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("fallo simulado")));
        a.RegistrarLecturaAsync(Arg.Any<AccionAuditada>(),Arg.Any<ActorAuditoria>(),Arg.Any<string>(),Arg.Any<IReadOnlyDictionary<string,object?>>(),Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("fallo simulado")));
        return a;
    }
}
