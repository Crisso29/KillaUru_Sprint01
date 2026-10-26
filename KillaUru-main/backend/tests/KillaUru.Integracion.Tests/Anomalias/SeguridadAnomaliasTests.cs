using System.Net;
using KillaUru.Integracion.Tests.Infraestructura;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>La administración no queda expuesta al incorporar rutas nuevas.</summary>
[Collection("Anomalias")]
public sealed class SeguridadAnomaliasTests(BaseDatosPrueba bd)
{
    [Theory]
    [InlineData("/api/v1/moderacion/pendientes")]
    [InlineData("/api/v1/moderacion/por-revisar")]
    [InlineData("/api/v1/anomalies/transiciones")]
    [InlineData("/api/v1/anomalies/00000000-0000-0000-0000-000000000001")]
    public async Task Requiere_autenticacion(string ruta)
    {
        using var api = new FabricaApi(bd); using var cliente = api.CreateClient();
        using var respuesta = await cliente.GetAsync(ruta, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }
    [Theory]
    [InlineData("/api/v1/moderacion/pendientes",false)]
    [InlineData("/api/v1/moderacion/por-revisar",false)]
    [InlineData("/api/v1/anomalies/transiciones",false)]
    [InlineData("/api/v1/anomalies/00000000-0000-0000-0000-000000000001",false)]
    [InlineData("/api/v1/anomalies/00000000-0000-0000-0000-000000000001/moderacion",true)]
    [InlineData("/api/v1/exportaciones",true)]
    public async Task Conductor_no_puede_usar_ninguna_operacion(string ruta,bool post)
    {
        using var api=new FabricaApi(bd); using var cliente=api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization=new("Bearer",Tokens.Emitir("ConductorRegistrado"));
        using var peticion=new HttpRequestMessage(post?HttpMethod.Post:HttpMethod.Get,ruta);
        if(post) peticion.Content=new StringContent("{}",System.Text.Encoding.UTF8,"application/json");
        using var respuesta=await cliente.SendAsync(peticion,TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden,respuesta.StatusCode);
    }
}
