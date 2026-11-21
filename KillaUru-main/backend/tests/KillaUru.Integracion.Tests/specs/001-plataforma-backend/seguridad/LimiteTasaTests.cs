using System.Net;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Domain;

namespace KillaUru.Integracion.Tests.Seguridad;

/// <summary>
/// Spec 001 — FR-010, FR-011 (T033, RNF-11, T-QA18):
/// cada rol tiene una cuota independiente; al superarla responde 429 con Retry-After.
/// Cada test crea su propia instancia de API para que los contadores no se compartan.
/// Valores configurados en ApiOptions: Anónimo 60 consultas/h y 120 lotes/h,
/// ConductorRegistrado 1 000 req/h, Administrador 3 000 req/h.
/// </summary>
public sealed class LimiteTasaTests(BaseDatosPrueba bd)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public Task Anonimo_60_consultas_y_la_61_devuelve_429() =>
        // Cuota exacta del anónimo para consultas (AnonimoConsultasPorHora = 60)
        AfirmarLimiteAsync(60, seudonimo => Peticiones.Get("/api/v1/prueba/publica", seudonimo: seudonimo));

    [Fact]
    public Task Anonimo_120_lotes_y_el_121_devuelve_429() =>
        // Cuota exacta del anónimo para lotes (AnonimoLotesPorHora = 120)
        AfirmarLimiteAsync(120, seudonimo => Peticiones.Post("/api/v1/prueba/lote", seudonimo: seudonimo));

    [Fact]
    public async Task La_cuota_de_lotes_es_independiente_de_la_de_consultas()
    {
        // Arrange — agotar la cuota de lotes con un seudónimo
        using var api = new FabricaApi(bd);
        var cliente = api.CreateClient();
        var seudonimo = Guid.NewGuid().ToString();
        await Peticiones.RepetirAsync(cliente, 120, () => Peticiones.Post("/api/v1/prueba/lote", seudonimo: seudonimo));

        // Act — hacer una consulta con el mismo seudónimo
        using var consulta = await cliente.SendAsync(
            Peticiones.Get("/api/v1/prueba/publica", seudonimo: seudonimo), Ct);

        // Assert — la cuota de consultas sigue intacta (FR-010: cuotas separadas por tipo)
        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
    }

    [Fact]
    public Task Conductor_1000_peticiones_y_la_1001_devuelve_429()
    {
        // La clave de la cuota es la cuenta, no la IP ni el seudónimo (FR-010)
        var token = Tokens.Emitir(Roles.ConductorRegistrado);
        return AfirmarLimiteAsync(1_000, _ => Peticiones.Get("/api/v1/prueba/publica", token));
    }

    [Fact]
    public Task Administrador_3000_peticiones_y_la_3001_devuelve_429()
    {
        // El administrador tiene cuota más alta (AdministradorPorHora = 3000, FR-010)
        var token = Tokens.Emitir(Roles.Administrador);
        return AfirmarLimiteAsync(3_000, _ => Peticiones.Get("/api/v1/prueba/publica", token));
    }

    [Fact]
    public async Task CF_Connecting_IP_se_ignora_fuera_de_la_red_del_tunel()
    {
        // Arrange — peticiones anónimas con CF-Connecting-IP diferente en cada una
        using var api = new FabricaApi(bd);
        var cliente = api.CreateClient();
        var seudonimo = Guid.NewGuid().ToString();

        // Act — enviar 61 peticiones; el header CF-Connecting-IP varía pero la IP real no
        var ultimo = await Peticiones.RepetirAsync(cliente, 61, () =>
        {
            var peticion = Peticiones.Get("/api/v1/prueba/publica", seudonimo: seudonimo);
            peticion.Headers.Add("CF-Connecting-IP", $"203.0.113.{Random.Shared.Next(1, 250)}");
            return peticion;
        });

        // Assert — el límite se aplica por IP real, no por CF-Connecting-IP (FR-011, ADR-010)
        Assert.Equal(HttpStatusCode.TooManyRequests, ultimo);
    }

    /// <summary>
    /// Verifica el valor límite exacto: <paramref name="limite"/> peticiones → 200,
    /// la siguiente → 429 con Retry-After positivo (T-QA18).
    /// </summary>
    private async Task AfirmarLimiteAsync(int limite, Func<string, HttpRequestMessage> crear)
    {
        // Arrange — API nueva con contadores en cero y seudónimo único
        using var api = new FabricaApi(bd);
        var cliente = api.CreateClient();
        var seudonimo = Guid.NewGuid().ToString();

        // Act — agotar la cuota exacta y hacer una petición más
        var enElLimite = await Peticiones.RepetirAsync(cliente, limite, () => crear(seudonimo));
        using var pasado = await cliente.SendAsync(crear(seudonimo), Ct);

        // Assert — la última dentro del límite es 200; la que supera, 429 con Retry-After (FR-010)
        Assert.Equal(HttpStatusCode.OK, enElLimite);
        await SeguridadTests.AfirmarProblemaAsync(pasado, HttpStatusCode.TooManyRequests, "limite_excedido");
        Assert.True(pasado.Headers.RetryAfter?.Delta?.TotalSeconds > 0,
            "Retry-After debe indicar cuándo puede reintentar (RNF-11)");
    }
}
