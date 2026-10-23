using System.Net;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Domain;

namespace KillaUru.Integracion.Tests.Seguridad;

/// <summary>
/// T065 (D2, D8): un script que cambia de seudónimo en cada petición no esquiva el techo por IP;
/// las peticiones con cuenta no cuentan para el techo; el registro tiene su propio límite por IP.
/// </summary>
public sealed class TechoPorIpTests(BaseDatosPrueba bd)
{
    private static string SeudonimoNuevo() => Guid.NewGuid().ToString();

    [Fact]
    public Task Seiscientos_lotes_por_IP_y_el_601_es_429_aunque_cambie_el_seudonimo() =>
        AfirmarTechoAsync(600, () => Peticiones.Post("/api/v1/prueba/lote", seudonimo: SeudonimoNuevo()));

    [Fact]
    public Task Trescientas_consultas_por_IP_y_la_301_es_429_aunque_cambie_el_seudonimo() =>
        AfirmarTechoAsync(300, () => Peticiones.Get("/api/v1/prueba/publica", seudonimo: SeudonimoNuevo()));

    [Fact]
    public async Task Las_peticiones_con_cuenta_no_cuentan_para_el_techo()
    {
        using var api = new FabricaApi(bd);
        var cliente = api.CreateClient();
        var token = Tokens.Emitir(Roles.ConductorRegistrado);
        await Peticiones.RepetirAsync(cliente, 300, () => Peticiones.Get("/api/v1/prueba/publica", token));

        var anonima = await Peticiones.RepetirAsync(cliente, 1, () => Peticiones.Get("/api/v1/prueba/publica", seudonimo: SeudonimoNuevo()));

        Assert.Equal(HttpStatusCode.OK, anonima);
    }

    [Fact]
    public Task Cinco_registros_por_IP_y_el_sexto_es_429() =>
        AfirmarTechoAsync(5, () => Peticiones.Post("/api/v1/prueba/registro", seudonimo: SeudonimoNuevo()));

    private async Task AfirmarTechoAsync(int techo, Func<HttpRequestMessage> crear)
    {
        using var api = new FabricaApi(bd);
        var cliente = api.CreateClient();

        var enElTecho = await Peticiones.RepetirAsync(cliente, techo, crear);
        var pasado = await Peticiones.RepetirAsync(cliente, 1, crear);

        Assert.Equal(HttpStatusCode.OK, enElTecho);
        Assert.Equal(HttpStatusCode.TooManyRequests, pasado);
    }
}
