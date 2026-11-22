using System.Net;
using System.Net.Http.Json;
using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Ingesta;

/// <summary>
/// Recepción por HTTP real contra PostgreSQL real: se comprueba la respuesta **y** las filas guardadas.
/// Escenarios del quickstart de 005 (columna «#»). Spec 005 · T012 · SC-004, FR-001, FR-007, FR-023…FR-025.
/// </summary>
public sealed class RecepcionApiTests(BaseDatosPrueba bd)
{
    // El procesamiento real es asíncrono (cola + trabajadores); el quickstart pide «en segundos».
    private static readonly TimeSpan PlazoProcesamiento = TimeSpan.FromSeconds(15);

    private static async Task<RespuestaLoteLeida> EnviarAsync(HttpClient cliente, HttpRequestMessage peticion)
    {
        using var respuesta = await cliente.SendAsync(peticion, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        return (await respuesta.Content.ReadFromJsonAsync<RespuestaLoteLeida>(TestContext.Current.CancellationToken))!;
    }

    /// <summary>#1 — 50 buenos: 200 con los 50 aceptados, guardados con el loteId del servidor y procesados.</summary>
    [Fact]
    public async Task Lote_de_50_buenos_se_guarda_entero_y_pasa_a_procesado()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var lote = new LoteDePrueba();
        var eventos = lote.Eventos(50);
        var ids = eventos.Select(e => (Guid)e["eventoId"]!).ToArray();

        // Act
        var respuesta = await EnviarAsync(api.CreateClient(), lote.Peticion(LoteDePrueba.Cuerpo(eventos)));
        var filas = await RecepcionEnBd.EsperarProcesadasAsync(bd.CadenaSuperusuario, ids, PlazoProcesamiento);

        // Assert — respuesta, filas guardadas con el loteId del servidor y estado final Procesado (1)
        Assert.Equal(ids.Order(), respuesta.Aceptados.Order());
        Assert.Empty(respuesta.Duplicados);
        Assert.Empty(respuesta.Rechazados);
        Assert.Equal(50, filas.Count);
        Assert.All(filas, f => Assert.Equal(respuesta.LoteId, f.LoteId));
        Assert.All(filas, f => Assert.Equal(1, f.Estado));
    }

    /// <summary>#3 — Reintento del mismo lote (la app no supo si llegó): 50 duplicados y ninguna fila nueva.</summary>
    [Fact]
    public async Task Reenviar_el_mismo_lote_devuelve_50_duplicados_sin_filas_nuevas()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var cliente = api.CreateClient();
        var lote = new LoteDePrueba();
        var eventos = lote.Eventos(50);
        var ids = eventos.Select(e => (Guid)e["eventoId"]!).ToArray();
        await EnviarAsync(cliente, lote.Peticion(LoteDePrueba.Cuerpo(eventos)));

        // Act
        var reintento = await EnviarAsync(cliente, lote.Peticion(LoteDePrueba.Cuerpo(eventos)));

        // Assert
        Assert.Empty(reintento.Aceptados);
        Assert.Equal(ids.Order(), reintento.Duplicados.Order());
        Assert.Equal(50, (await RecepcionEnBd.LeerAsync(bd.CadenaSuperusuario, ids)).Count);
    }

    /// <summary>#4 — El mismo eventoId dos veces en un lote: se guarda una vez; la segunda copia es duplicado.</summary>
    [Fact]
    public async Task Mismo_eventoId_dos_veces_en_un_lote_es_un_aceptado_y_un_duplicado()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var lote = new LoteDePrueba();
        var id = Guid.CreateVersion7();

        // Act
        var respuesta = await EnviarAsync(api.CreateClient(), lote.Peticion(LoteDePrueba.Cuerpo([lote.Evento(id), lote.Evento(id)])));

        // Assert
        Assert.Equal([id], respuesta.Aceptados);
        Assert.Equal([id], respuesta.Duplicados);
        Assert.Single(await RecepcionEnBd.LeerAsync(bd.CadenaSuperusuario, [id]));
    }

    /// <summary>#21 — Un `loteId` en el cuerpo se ignora: el servidor genera otro, UUIDv7 (D3).</summary>
    [Fact]
    public async Task LoteId_del_cliente_se_ignora_y_el_servidor_genera_un_UUIDv7()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var lote = new LoteDePrueba();
        var delCliente = Guid.CreateVersion7();

        // Act
        var respuesta = await EnviarAsync(api.CreateClient(), lote.Peticion(LoteDePrueba.Cuerpo(lote.Eventos(1), delCliente)));

        // Assert
        Assert.NotEqual(delCliente, respuesta.LoteId);
        Assert.Equal(7, respuesta.LoteId.Version);
    }
}
