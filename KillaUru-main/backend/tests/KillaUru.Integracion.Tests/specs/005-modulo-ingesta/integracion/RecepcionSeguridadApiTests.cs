using System.Net;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Domain;

namespace KillaUru.Integracion.Tests.Ingesta;

/// <summary>
/// Quién puede enviar, con qué peso (`con_sesion`) y con qué seudónimo. Escenarios #9, #10, #11, #18, #19 y #20 del
/// quickstart de 005. Spec 005 · T012 · FR-001, FR-024 (RN-14, D2, D8, R-004).
/// </summary>
public sealed class RecepcionSeguridadApiTests(BaseDatosPrueba bd)
{
    private static Guid IdDe(System.Text.Json.Nodes.JsonObject evento) => (Guid)evento["eventoId"]!;

    /// <summary>#9 — Token vencido hace 1 s: 401 y nada guardado (no se trata como anónimo).</summary>
    [Fact]
    public async Task Token_vencido_responde_401_y_no_guarda()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var lote = new LoteDePrueba();
        var eventos = lote.Eventos(2);
        var vencido = Tokens.Emitir(Roles.ConductorRegistrado, venceEn: TimeSpan.FromSeconds(-1), verificado: true);

        // Act
        using var respuesta = await api.CreateClient().SendAsync(lote.Peticion(LoteDePrueba.Cuerpo(eventos), vencido), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        Assert.Empty(await RecepcionEnBd.LeerAsync(bd.CadenaSuperusuario, eventos.Select(IdDe)));
    }

    /// <summary>#11 — El administrador no detecta: 403 `sin_permiso` y nada guardado (R-004).</summary>
    [Fact]
    public async Task Administrador_responde_403_sin_permiso_y_no_guarda()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var lote = new LoteDePrueba();
        var eventos = lote.Eventos(2);
        var token = Tokens.Emitir(Roles.Administrador, verificado: true);

        // Act
        using var respuesta = await api.CreateClient().SendAsync(lote.Peticion(LoteDePrueba.Cuerpo(eventos), token), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        Assert.Equal("sin_permiso", await ProblemaLeido.CodigoAsync(respuesta));
        Assert.Empty(await RecepcionEnBd.LeerAsync(bd.CadenaSuperusuario, eventos.Select(IdDe)));
    }

    /// <summary>
    /// #10 y #20 — `con_sesion` se guarda true solo con un conductor registrado **verificado**; sin verificar
    /// o sin token es false (pesa como anónimo en la confirmación, RN-14, D8).
    /// </summary>
    [Theory]
    [InlineData(Roles.ConductorRegistrado, true, true)]
    [InlineData(Roles.ConductorRegistrado, false, false)]
    [InlineData(null, false, false)]
    public async Task Con_sesion_depende_del_rol_y_de_verificado(string? rol, bool verificado, bool conSesion)
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var lote = new LoteDePrueba();
        var eventos = lote.Eventos(1);
        var token = rol is null ? null : Tokens.Emitir(rol, verificado: verificado);

        // Act
        await EnvioLote.EnviarAsync(api.CreateClient(), lote.Peticion(LoteDePrueba.Cuerpo(eventos), token));

        // Assert
        var fila = Assert.Single(await RecepcionEnBd.LeerAsync(bd.CadenaSuperusuario, eventos.Select(IdDe)));
        Assert.Equal(conSesion, fila.ConSesion);
    }

    /// <summary>#18 — Cabecera con seudónimo S y un evento con T: ese evento sale SeudonimoInconsistente, sin fila (D2).</summary>
    [Fact]
    public async Task Evento_con_otro_seudonimo_que_la_cabecera_es_inconsistente()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var lote = new LoteDePrueba();
        var eventos = lote.Eventos(3);
        eventos[2]["seudonimo"] = Guid.NewGuid();

        // Act
        var respuesta = await EnvioLote.EnviarAsync(api.CreateClient(), lote.Peticion(LoteDePrueba.Cuerpo(eventos)));

        // Assert
        Assert.Equal(new RechazoLeido(IdDe(eventos[2]), 2, "SeudonimoInconsistente"), Assert.Single(respuesta.Rechazados));
        Assert.Equal(2, (await RecepcionEnBd.LeerAsync(bd.CadenaSuperusuario, eventos.Select(IdDe))).Count);
    }

    /// <summary>#19 — Sin cabecera y eventos con S, S, T: el lote toma S (el primero válido) y T sale inconsistente.</summary>
    [Fact]
    public async Task Sin_cabecera_el_lote_toma_el_seudonimo_del_primer_evento()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var lote = new LoteDePrueba();
        var eventos = lote.Eventos(3);
        eventos[2]["seudonimo"] = Guid.NewGuid();

        // Act
        var respuesta = await EnvioLote.EnviarAsync(
            api.CreateClient(), lote.Peticion(LoteDePrueba.Cuerpo(eventos), conCabeceraSeudonimo: false));

        // Assert
        Assert.Equal(new RechazoLeido(IdDe(eventos[2]), 2, "SeudonimoInconsistente"), Assert.Single(respuesta.Rechazados));
        Assert.Equal(eventos[..2].Select(IdDe).Order(), respuesta.Aceptados.Order());
    }
}
