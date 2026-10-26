using System.Net.Http.Json;
using System.Text.Json;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Auditoria.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>Matriz RN-07 por HTTP y auditoría del mismo commit.</summary>
[Collection("Anomalias")]
public sealed class ModeracionApiTests(BaseDatosPrueba bd)
{
    public static IEnumerable<object[]> Casos()
    {
        var acciones = new[] { "MarcarReparada", "Descartar", "Reabrir", "MantenerDescartada" };
        int?[,] esperado = { {4,5,null,null}, {4,5,null,null}, {null,5,null,null}, {null,null,1,null}, {null,null,1,5} };
        for (var estado=1; estado<=5; estado++)
            for (var accion=0; accion<4; accion++) yield return new object[] { estado, acciones[accion], esperado[estado-1,accion]! };
    }
    [Theory, MemberData(nameof(Casos))]
    public async Task Matriz_completa_y_auditoria(int estado, string accion, int? destino)
    {
        using var e = new EscenarioAnomalias(bd); var evento = e.Evento(); await e.ProcesarAsync(evento);
        await using var scope = e.Api.Services.CreateAsyncScope(); var ct = TestContext.Current.CancellationToken;
        var db = scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        var id = (await db.Reportes.SingleAsync(r => r.EventoId == evento.EventoId, ct)).AnomaliaId;
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE anomalias.anomalia SET estado={(short)estado}, por_revisar={estado==5} WHERE id={id}", ct);
        var actor = Guid.NewGuid(); using var cliente = e.Api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", Tokens.Emitir("Administrador", actor.ToString(), reloj:e.Reloj));
        using var respuesta = await cliente.PostAsJsonAsync($"/api/v1/anomalies/{id}/moderacion", new { accion }, ct);
        Assert.Equal(destino is null ? 409 : 200, (int)respuesta.StatusCode);
        var actual = await db.Anomalias.AsNoTracking().SingleAsync(a => a.Id == id, ct);
        Assert.Equal(destino ?? estado, (int)actual.Estado);
        var auditoria = scope.ServiceProvider.GetRequiredService<AuditoriaDbContext>();
        Assert.Equal(destino is null ? 0 : 1, await auditoria.Registros.CountAsync(r => r.ActorId == actor && r.Objetivo == id.ToString(), ct));
        if (destino is >= 3)
        {
            using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(ct));
            Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("venceUtc").ValueKind);
        }
    }
}
