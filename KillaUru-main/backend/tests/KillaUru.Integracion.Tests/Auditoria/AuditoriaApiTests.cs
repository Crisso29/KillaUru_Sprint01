using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;
namespace KillaUru.Integracion.Tests.Auditoria;

/// <summary>Contrato HTTP del módulo real: acceso, filtros, errores y ausencia de escritura pública.</summary>
public sealed class AuditoriaApiTests(BaseDatosPrueba bd)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(null, 401)] [InlineData("ConductorRegistrado", 403)] [InlineData("Administrador", 200)]
    public async Task Solo_el_administrador_consulta(string? rol, int estado)
    {
        using var api = new FabricaApi(bd);
        using var cliente = api.CreateClient();
        if (rol is not null) cliente.DefaultRequestHeaders.Authorization = new("Bearer", Tokens.Emitir(rol));
        using var respuesta = await cliente.GetAsync("/api/v1/auditoria", Ct);
        Assert.Equal(estado, (int)respuesta.StatusCode);
        Assert.True(respuesta.Headers.CacheControl!.NoStore);
    }

    [Theory]
    [InlineData("tamano=201")] [InlineData("tamano=abc")] [InlineData("categoria=otra")]
    [InlineData("desde=2026-10-23T00:00:00Z&hasta=2026-10-22T00:00:00Z")]
    public async Task Filtros_invalidos_devuelven_problem_details(string filtros)
    {
        using var api = new FabricaApi(bd);
        using var cliente = api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", Tokens.Emitir("Administrador"));
        using var respuesta = await cliente.GetAsync("/api/v1/auditoria?" + filtros, Ct);
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(Ct));
        Assert.Equal("datos_invalidos", json.RootElement.GetProperty("codigo").GetString());
    }

    [Theory]
    [InlineData("PUT")] [InlineData("PATCH")] [InlineData("DELETE")]
    public async Task No_existen_operaciones_de_modificacion(string metodo)
    {
        using var api = new FabricaApi(bd);
        using var peticion = new HttpRequestMessage(new HttpMethod(metodo), "/api/v1/auditoria");
        peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Tokens.Emitir("Administrador"));
        using var respuesta = await api.CreateClient().SendAsync(peticion, Ct);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, respuesta.StatusCode);
    }

    [Fact]
    public async Task La_consulta_se_audita_con_actor_y_filtros()
    {
        var actor = Guid.NewGuid();
        using var api = new FabricaApi(bd);
        using var cliente = api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", Tokens.Emitir("Administrador", actor.ToString()));
        var ruta = $"/api/v1/auditoria?actorId={actor}&accion=AccesoAdministrativoDatos&tamano=1";
        using var antes = JsonDocument.Parse(await cliente.GetStringAsync(ruta, Ct));
        Assert.Equal(0, antes.RootElement.GetProperty("total").GetInt64());
        using var despues = JsonDocument.Parse(await cliente.GetStringAsync(ruta, Ct));
        Assert.Equal(1, despues.RootElement.GetProperty("total").GetInt64());
        Assert.Equal("auditoria", despues.RootElement.GetProperty("elementos")[0].GetProperty("objetivo").GetString());
    }
}
