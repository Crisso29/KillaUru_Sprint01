using KillaUru.Anomalias.Application.Exportacion;
using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>Una modificación entre contar y leer no cambia las filas de la exportación auditada.</summary>
[Collection("Anomalias")]
public sealed class InstantaneaExportacionTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Conteo_y_lectura_conservan_snapshot_ante_cambios_concurrentes()
    {
        using var e=new EscenarioAnomalias(bd);var evento=e.Evento();await e.ProcesarAsync(evento);
        await using var lector=e.Api.Services.CreateAsyncScope();await using var escritor=e.Api.Services.CreateAsyncScope();
        var ct=TestContext.Current.CancellationToken;var db=escritor.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        var id=(await db.Reportes.SingleAsync(r=>r.EventoId==evento.EventoId,ct)).AnomaliaId;
        var repo=lector.ServiceProvider.GetRequiredService<IExportacionRepository>();
        await using var snapshot=await repo.AbrirInstantaneaAsync(ct);
        var filtro=new SolicitudExportacion("CSV",Estados:["Activa"]);var cantidad=await repo.ContarAsync(filtro,ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE anomalias.anomalia SET estado=5 WHERE id={id}",ct);
        var filas=new List<FilaExportacion>();await foreach(var fila in repo.LeerAsync(filtro,ct)) filas.Add(fila);
        Assert.Equal(cantidad,filas.Count);Assert.Contains(filas,f=>f.Id==id&&f.Estado=="Activa");
    }
}
