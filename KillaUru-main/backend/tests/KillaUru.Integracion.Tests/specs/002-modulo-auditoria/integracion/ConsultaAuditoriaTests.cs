using System.Net;
using System.Text.Json;
using KillaUru.Api.Configuracion;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Auditoria;

/// <summary>Verifica filtros combinados, desempate estable y cuota de administrador sobre el endpoint real.</summary>
public sealed class ConsultaAuditoriaTests(BaseDatosPrueba bd)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Orden_y_paginacion_respetan_todos_los_filtros()
    {
        var actor = Guid.NewGuid();
        await SemillasAuditoria.InsertarAsync(bd.CadenaApp, actor, 5);
        using var api = new FabricaApi(bd);
        using var cliente = api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", Tokens.Emitir("Administrador"));
        var ruta = $"/api/v1/auditoria?actorId={actor}&categoria=Usuarios&accion=UsuarioCreado&tamano=2&desde=2026-10-01T00:00:00Z&hasta=2026-10-31T23:59:59Z";
        using var primera = JsonDocument.Parse(await cliente.GetStringAsync(ruta, Ct));
        using var segunda = JsonDocument.Parse(await cliente.GetStringAsync(ruta + "&pagina=2", Ct));
        var ids = primera.RootElement.GetProperty("elementos").EnumerateArray().Concat(
            segunda.RootElement.GetProperty("elementos").EnumerateArray()).Select(e => e.GetProperty("id").GetInt64()).ToArray();
        Assert.Equal(5, primera.RootElement.GetProperty("total").GetInt64());
        Assert.Equal(4, ids.Distinct().Count());
        Assert.Equal(ids.OrderDescending(), ids);
    }

    [Fact]
    public async Task Superar_cuota_devuelve_429_con_retry_after()
    {
        using var original = new FabricaApi(bd);
        using var api = original.WithWebHostBuilder(builder => builder.UseSetting("Api:LimiteTasa:AdministradorPorHora", "2"));
        using var cliente = api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", Tokens.Emitir("Administrador"));
        for (var i = 0; i < 2; i++) Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/api/v1/auditoria", Ct)).StatusCode);
        using var respuesta = await cliente.GetAsync("/api/v1/auditoria", Ct);
        Assert.Equal(HttpStatusCode.TooManyRequests, respuesta.StatusCode);
        Assert.NotNull(respuesta.Headers.RetryAfter);
    }
}
