using System.Net;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Domain;

namespace KillaUru.Integracion.Tests.Seguridad;

/// <summary>
/// T033 (RNF-11, T-QA18): cada cuota en su valor límite, el siguiente → 429 con Retry-After. Cada prueba
/// crea su propia API para no compartir contadores.
/// </summary>
public sealed class LimiteTasaTests(BaseDatosPrueba bd)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public Task Anonimo_60_consultas_y_la_61_es_429() =>
        AfirmarLimiteAsync(60, seudonimo => Peticiones.Get("/api/v1/prueba/publica", seudonimo: seudonimo));

    [Fact]
    public Task Anonimo_120_lotes_y_el_121_es_429() =>
        AfirmarLimiteAsync(120, seudonimo => Peticiones.Post("/api/v1/prueba/lote", seudonimo: seudonimo));

    [Fact]
    public async Task La_cuota_de_lotes_no_consume_la_de_consultas()
    {
        using var api = new FabricaApi(bd);
        var cliente = api.CreateClient();
        var seudonimo = Guid.NewGuid().ToString();
        await Peticiones.RepetirAsync(cliente, 120, () => Peticiones.Post("/api/v1/prueba/lote", seudonimo: seudonimo));

        using var consulta = await cliente.SendAsync(Peticiones.Get("/api/v1/prueba/publica", seudonimo: seudonimo), Ct);

        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
    }

    [Fact]
    public Task Conductor_1000_y_la_1001_es_429()
    {
        var token = Tokens.Emitir(Roles.ConductorRegistrado);
        return AfirmarLimiteAsync(1_000, _ => Peticiones.Get("/api/v1/prueba/publica", token));
    }

    [Fact]
    public Task Administrador_3000_y_la_3001_es_429()
    {
        var token = Tokens.Emitir(Roles.Administrador);
        return AfirmarLimiteAsync(3_000, _ => Peticiones.Get("/api/v1/prueba/publica", token));
    }

    [Fact]
    public async Task CF_Connecting_IP_se_ignora_fuera_de_la_red_del_tunel()
    {
        using var api = new FabricaApi(bd);
        var cliente = api.CreateClient();
        var seudonimo = Guid.NewGuid().ToString();
        var ultimo = await Peticiones.RepetirAsync(cliente, 61, () =>
        {
            var peticion = Peticiones.Get("/api/v1/prueba/publica", seudonimo: seudonimo);
            peticion.Headers.Add("CF-Connecting-IP", $"203.0.113.{Random.Shared.Next(1, 250)}");   // IP inventada en cada una
            return peticion;
        });

        Assert.Equal(HttpStatusCode.TooManyRequests, ultimo);
    }

    private async Task AfirmarLimiteAsync(int limite, Func<string, HttpRequestMessage> crear)
    {
        using var api = new FabricaApi(bd);
        var cliente = api.CreateClient();
        var seudonimo = Guid.NewGuid().ToString();

        var enElLimite = await Peticiones.RepetirAsync(cliente, limite, () => crear(seudonimo));
        using var pasado = await cliente.SendAsync(crear(seudonimo), Ct);

        Assert.Equal(HttpStatusCode.OK, enElLimite);
        await SeguridadTests.AfirmarProblemaAsync(pasado, HttpStatusCode.TooManyRequests, "limite_excedido");
        Assert.True(pasado.Headers.RetryAfter?.Delta?.TotalSeconds > 0);
    }
}
