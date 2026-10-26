using System.Net;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Domain;

namespace KillaUru.Integracion.Tests.Seguridad;

/// <summary>
/// Spec 001 — FR-030, FR-031 (T065, D2, D8):
/// el techo por IP impide que un script rote seudónimos para evadir el límite.
/// La clave del techo es la IP real, independiente del seudónimo y de la cuenta autenticada.
/// Valores: TechoIpLotesPorHora = 600, TechoIpConsultasPorHora = 300, RegistrosPorHoraPorIp = 5.
/// </summary>
public sealed class TechoPorIpTests(BaseDatosPrueba bd)
{
    // Seudónimo diferente en cada petición para simular script de evasión (D2)
    private static string SeudonimoNuevo() => Guid.NewGuid().ToString();

    [Fact]
    public Task Seiscientos_lotes_anonimos_por_IP_y_el_601_es_429_aunque_rote_seudonimo() =>
        // TechoIpLotesPorHora = 600 (FR-030); el cambio de seudónimo no reinicia el contador
        AfirmarTechoAsync(600, () => Peticiones.Post("/api/v1/prueba/lote", seudonimo: SeudonimoNuevo()));

    [Fact]
    public Task Trescientas_consultas_anonimas_por_IP_y_la_301_es_429_aunque_rote_seudonimo() =>
        // TechoIpConsultasPorHora = 300 (FR-030); mismo principio que los lotes
        AfirmarTechoAsync(300, () => Peticiones.Get("/api/v1/prueba/publica", seudonimo: SeudonimoNuevo()));

    [Fact]
    public async Task Peticiones_autenticadas_no_consumen_el_techo_por_IP_de_anonimos()
    {
        // Arrange — un conductor autenticado agota las 300 consultas que le corresponden
        using var api = new FabricaApi(bd);
        var cliente = api.CreateClient();
        var token = Tokens.Emitir(Roles.ConductorRegistrado);
        await Peticiones.RepetirAsync(cliente, 300, () => Peticiones.Get("/api/v1/prueba/publica", token));

        // Act — una petición anónima desde la misma IP
        var anonima = await Peticiones.RepetirAsync(cliente, 1,
            () => Peticiones.Get("/api/v1/prueba/publica", seudonimo: SeudonimoNuevo()));

        // Assert — el techo anónimo no se tocó: la petición autenticada usa la cuota de cuenta (D2, FR-030)
        Assert.Equal(HttpStatusCode.OK, anonima);
    }

    [Fact]
    public Task Cinco_registros_por_IP_y_el_sexto_es_429() =>
        // RegistrosPorHoraPorIp = 5 (FR-031, D8): mitiga la enumeración de correos por fuerza bruta
        AfirmarTechoAsync(5, () => Peticiones.Post("/api/v1/prueba/registro", seudonimo: SeudonimoNuevo()));

    /// <summary>
    /// Verifica el valor del techo exacto: <paramref name="techo"/> peticiones → 200,
    /// la siguiente → 429 (el techo se aplica por IP real, sin importar el seudónimo).
    /// </summary>
    private async Task AfirmarTechoAsync(int techo, Func<HttpRequestMessage> crear)
    {
        // Arrange — API nueva con contadores en cero
        using var api = new FabricaApi(bd);
        var cliente = api.CreateClient();

        // Act — llegar al techo y superarlo en una petición
        var enElTecho = await Peticiones.RepetirAsync(cliente, techo, crear);
        var pasado = await Peticiones.RepetirAsync(cliente, 1, crear);

        // Assert — la petición en el límite exacto pasa; la siguiente es rechazada (FR-030/FR-031)
        Assert.Equal(HttpStatusCode.OK, enElTecho);
        Assert.Equal(HttpStatusCode.TooManyRequests, pasado);
    }
}
