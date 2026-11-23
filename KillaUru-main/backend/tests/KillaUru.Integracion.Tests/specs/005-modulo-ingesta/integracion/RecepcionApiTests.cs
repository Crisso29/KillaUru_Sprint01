using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Ingesta;

/// <summary>
/// Recepción por HTTP real contra PostgreSQL real: se comprueba la respuesta **y** las filas guardadas.
/// Escenarios del quickstart de 005 (columna «#»). Spec 005 · T012 · SC-004, FR-001, FR-007, FR-023…FR-025.
/// </summary>
public sealed class RecepcionApiTests(BaseDatosPrueba bd)
{
    // El procesamiento real es asíncrono (cola + trabajadores). Es el TOPE para declarar la falla, no lo que se espera:
    // el sondeo sale en cuanto todo está procesado. Un lote lo procesa un solo trabajador, evento tras evento, a
    // ~186 ms cada uno (DT-01): 50 eventos ≈ 9 s. Medido el 2026-11-23: 11,3 s en la suite completa y 17 s con el
    // equipo cargado; con 15 s la prueba fallaba de vez en cuando. 60 s ≈ 3,5 veces el peor caso medido.
    private static readonly TimeSpan PlazoProcesamiento = TimeSpan.FromSeconds(60);

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
        var respuesta = await EnvioLote.EnviarAsync(api.CreateClient(), lote.Peticion(LoteDePrueba.Cuerpo(eventos)));
        var filas = await RecepcionEnBd.EsperarProcesadasAsync(bd.CadenaSuperusuario, ids, PlazoProcesamiento);

        // Assert — respuesta, filas guardadas con el loteId del servidor y estado final Procesado (1)
        Assert.Equal(ids.Order(), respuesta.Aceptados.Order());
        Assert.Empty(respuesta.Duplicados);
        Assert.Empty(respuesta.Rechazados);
        Assert.Equal(50, filas.Count);
        Assert.All(filas, f => Assert.Equal(respuesta.LoteId, f.LoteId));
        var pendientes = filas.Count(f => f.Estado == 0);
        Assert.True(pendientes == 0, $"{pendientes} de 50 siguen en Recibido tras {PlazoProcesamiento.TotalSeconds} s (DT-01)");
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
        await EnvioLote.EnviarAsync(cliente, lote.Peticion(LoteDePrueba.Cuerpo(eventos)));

        // Act
        var reintento = await EnvioLote.EnviarAsync(cliente, lote.Peticion(LoteDePrueba.Cuerpo(eventos)));

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
        var respuesta = await EnvioLote.EnviarAsync(api.CreateClient(), lote.Peticion(LoteDePrueba.Cuerpo([lote.Evento(id), lote.Evento(id)])));

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
        var respuesta = await EnvioLote.EnviarAsync(api.CreateClient(), lote.Peticion(LoteDePrueba.Cuerpo(lote.Eventos(1), delCliente)));

        // Assert
        Assert.NotEqual(delCliente, respuesta.LoteId);
        Assert.Equal(7, respuesta.LoteId.Version);
    }
}
