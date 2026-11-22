using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Ingesta;

/// <summary>
/// Un evento malo se rechaza solo, sin hacer fallar el lote, y no deja fila (RN-11, D5). Escenarios #2, #5, #6
/// y #17 del quickstart de 005; el #5 prueba los dos lados de cada límite. Spec 005 · T012 · FR-002, FR-006.
/// </summary>
public sealed class RecepcionRechazosApiTests(BaseDatosPrueba bd)
{
    /// <summary>#2 — 50 eventos, uno sin severidad: 49 aceptados y ese rechazado por su posición.</summary>
    [Fact]
    public async Task Evento_sin_severidad_se_rechaza_solo_y_los_otros_49_se_aceptan()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var lote = new LoteDePrueba();
        var eventos = lote.Eventos(50);
        eventos[13].Remove("severidad");
        var malo = (Guid)eventos[13]["eventoId"]!;

        // Act
        var respuesta = await EnvioLote.EnviarAsync(api.CreateClient(), lote.Peticion(LoteDePrueba.Cuerpo(eventos)));

        // Assert — el rechazado no tiene fila; los 49 sí
        Assert.Equal(new RechazoLeido(malo, 13, "EsquemaInvalido"), Assert.Single(respuesta.Rechazados));
        Assert.Equal(49, respuesta.Aceptados.Length);
        var filas = await RecepcionEnBd.LeerAsync(bd.CadenaSuperusuario, eventos.Select(e => (Guid)e["eventoId"]!));
        Assert.Equal(respuesta.Aceptados.Order(), filas.Select(f => f.EventoId).Order());
    }

    /// <summary>#6 — `"severidad": "alta"` (otro tipo): solo ese evento sale EsquemaInvalido.</summary>
    [Fact]
    public async Task Severidad_de_texto_rechaza_solo_ese_evento()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var lote = new LoteDePrueba();
        var eventos = lote.Eventos(3);
        eventos[1]["severidad"] = "alta";

        // Act
        var respuesta = await EnvioLote.EnviarAsync(api.CreateClient(), lote.Peticion(LoteDePrueba.Cuerpo(eventos)));

        // Assert
        Assert.Equal(new RechazoLeido((Guid)eventos[1]["eventoId"]!, 1, "EsquemaInvalido"), Assert.Single(respuesta.Rechazados));
        Assert.Equal(2, respuesta.Aceptados.Length);
    }

    /// <summary>
    /// #5 — Los límites de RN-11 son inclusivos: a 50 m de la vía y con confidence 0,75 se acepta;
    /// a 51 m sale FueraDelCorredor y con 0,74 ConfidenceBajo, sin fila en ambos casos.
    /// </summary>
    [Theory]
    [InlineData(51, 0.91, "FueraDelCorredor")]
    [InlineData(0, 0.74, "ConfidenceBajo")]
    [InlineData(50, 0.91, null)]
    [InlineData(0, 0.75, null)]
    public async Task Limites_de_distancia_y_confidence_deciden_el_rechazo(double metrosDeLaVia, double confidence, string? motivo)
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var lote = new LoteDePrueba();
        var id = Guid.CreateVersion7();
        var evento = lote.Evento(id);
        evento["latitud"] = LoteDePrueba.LatitudAMetrosDeLaVia(metrosDeLaVia);
        evento["confidence"] = confidence;

        // Act
        var respuesta = await EnvioLote.EnviarAsync(api.CreateClient(), lote.Peticion(LoteDePrueba.Cuerpo([evento])));

        // Assert
        var filas = await RecepcionEnBd.LeerAsync(bd.CadenaSuperusuario, [id]);
        Assert.Equal(motivo, respuesta.Rechazados.SingleOrDefault()?.Motivo);
        Assert.Equal(motivo is null ? 1 : 0, filas.Count);
    }

    /// <summary>#17 — Sin eventoId en la posición 7: se informa con `id: null` y su índice para que la app lo ubique.</summary>
    [Fact]
    public async Task Evento_sin_eventoId_se_informa_con_id_null_y_su_indice()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var lote = new LoteDePrueba();
        var eventos = lote.Eventos(10);
        eventos[7].Remove("eventoId");

        // Act
        var respuesta = await EnvioLote.EnviarAsync(api.CreateClient(), lote.Peticion(LoteDePrueba.Cuerpo(eventos)));

        // Assert
        Assert.Equal(new RechazoLeido(null, 7, "EsquemaInvalido"), Assert.Single(respuesta.Rechazados));
        Assert.Equal(9, respuesta.Aceptados.Length);
    }
}
