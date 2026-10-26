using System.Text.Json;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>Las listas entregan los campos necesarios para decidir sin divulgar dispositivos.</summary>
[Collection("Anomalias")]
public sealed class ListasAdministracionTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Pendientes_revision_detalle_y_transiciones_tienen_datos_coherentes()
    {
        using var e=new EscenarioAnomalias(bd);var evento=e.Evento(5);await e.ProcesarAsync(evento);
        await using var scope=e.Api.Services.CreateAsyncScope();var ct=TestContext.Current.CancellationToken;
        var db=scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();var id=(await db.Reportes.SingleAsync(r=>r.EventoId==evento.EventoId,ct)).AnomaliaId;
        using var cliente=e.Api.CreateClient();cliente.DefaultRequestHeaders.Authorization=new("Bearer",Tokens.Emitir("Administrador",reloj:e.Reloj));
        using var pendientes=JsonDocument.Parse(await cliente.GetStringAsync("/api/v1/moderacion/pendientes",ct));
        var pendiente=pendientes.RootElement.EnumerateArray().Single(a=>a.GetProperty("id").GetGuid()==id);
        Assert.Equal(5,pendiente.GetProperty("severidadMostrada").GetInt32());Assert.Equal(1,pendiente.GetProperty("reportes").GetInt32());
        var ahora=e.Reloj.GetUtcNow();
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE anomalias.anomalia SET estado=5,por_revisar=true,marcas_revision=ARRAY[{ahora},{ahora},{ahora}] WHERE id={id}",ct);
        using var revisiones=JsonDocument.Parse(await cliente.GetStringAsync("/api/v1/moderacion/por-revisar",ct));
        var revision=revisiones.RootElement.EnumerateArray().Single(a=>a.GetProperty("id").GetGuid()==id);
        Assert.Equal(5,revision.GetProperty("severidadMostrada").GetInt32());Assert.Equal(3,revision.GetProperty("vehiculos7d").GetInt32());
        using var detalle=JsonDocument.Parse(await cliente.GetStringAsync($"/api/v1/anomalies/{id}",ct));
        Assert.Equal(JsonValueKind.Null,detalle.RootElement.GetProperty("venceUtc").ValueKind);Assert.Equal(3,detalle.RootElement.GetProperty("vehiculosRevision").GetInt32());
        using var cambios=JsonDocument.Parse(await cliente.GetStringAsync("/api/v1/anomalies/transiciones?limite=1",ct));
        Assert.Equal(1,cambios.RootElement.GetArrayLength());Assert.True(cambios.RootElement[0].TryGetProperty("actorId",out _));
    }
}
