using System.Net;
using KillaUru.Anomalias.Contracts;
using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Mapa;

/// <summary>
/// T024 — filtros del mapa con HTTP y PostgreSQL reales: el administrador ve los 5 estados y filtra; el conductor
/// filtra por severidad pero no por estado; nadie más pide `estado` (T-SEG20). Las respuestas filtradas no llevan
/// `ETag` (no se cachean por celda). Escenarios 5–8 y 13 del quickstart. Spec 006 · RF-17, FR-010, FR-011.
/// </summary>
[Collection("Anomalias")]
public sealed class FiltrosApiTests(BaseDatosPrueba bd)
{
    private static readonly EstadoAnomalia[] EstadosFinales =
        [EstadoAnomalia.Expirada, EstadoAnomalia.Reparada, EstadoAnomalia.Descartada];

    [Fact]
    public async Task Administrador_sin_filtros_ve_los_5_estados_y_los_finales_sin_vencimiento()
    {
        // Arrange — escenario 6
        using var mapa = new EscenarioMapa(bd);
        var porEstado = new Dictionary<Guid, EstadoAnomalia>();
        foreach (var estado in Enum.GetValues<EstadoAnomalia>())
            porEstado[await mapa.CrearAsync(EscenarioMapa.LongitudFuera(), estado)] = estado;

        // Act
        var respuesta = await RespuestaMapa.PedirAsync(mapa.Administrador());

        // Assert — cada una con su estado; D6: Expirada, Reparada y Descartada llevan venceUtc: null
        var mias = respuesta.Anomalias.Where(anomalia => porEstado.ContainsKey(anomalia.Id)).ToList();
        Assert.Equal(5, mias.Count);
        Assert.All(mias, anomalia => Assert.Equal(porEstado[anomalia.Id].ToString(), anomalia.Estado));
        Assert.All(mias, anomalia => Assert.Equal(EstadosFinales.Contains(porEstado[anomalia.Id]), anomalia.VenceNulo));
        Assert.True(respuesta.EsPrivadaSinCache);
        Assert.False(respuesta.TieneETag);
    }

    [Fact]
    public async Task Administrador_filtra_por_estado_y_tipo()
    {
        // Arrange — escenario 7: solo las descartadas de tipo bache
        using var mapa = new EscenarioMapa(bd);
        var buscada = await mapa.CrearAsync(EscenarioMapa.LongitudFuera(), EstadoAnomalia.Descartada);
        var otroTipo = await mapa.CrearAsync(EscenarioMapa.LongitudFuera(), EstadoAnomalia.Descartada, tipo: "Hundimiento");
        var otroEstado = await mapa.CrearAsync(EscenarioMapa.LongitudFuera(), EstadoAnomalia.Reparada);

        // Act
        var respuesta = await RespuestaMapa.PedirAsync(mapa.Administrador(), "?estado=Descartada&tipo=Bache");

        // Assert
        Assert.Contains(buscada, respuesta.Ids);
        Assert.DoesNotContain(otroTipo, respuesta.Ids);
        Assert.DoesNotContain(otroEstado, respuesta.Ids);
        Assert.All(respuesta.Anomalias, anomalia => Assert.Equal(("Descartada", "Bache"), (anomalia.Estado, anomalia.Tipo)));
    }

    [Fact]
    public async Task Conductor_filtra_por_severidad_solo_entre_las_activas_y_sin_ETag()
    {
        // Arrange — escenario 8
        using var mapa = new EscenarioMapa(bd);
        var grave = await mapa.CrearAsync(EscenarioMapa.LongitudFuera(), severidad: 5);
        var leve = await mapa.CrearAsync(EscenarioMapa.LongitudFuera(), severidad: 3);
        var graveExpirada = await mapa.CrearAsync(EscenarioMapa.LongitudFuera(), EstadoAnomalia.Expirada, severidad: 5);

        // Act
        var respuesta = await RespuestaMapa.PedirAsync(mapa.Conductor(), "?severidad=4,5");

        // Assert — el conductor no ve el estado (FR-010) ni anomalías que no estén activas
        Assert.Contains(grave, respuesta.Ids);
        Assert.DoesNotContain(leve, respuesta.Ids);
        Assert.DoesNotContain(graveExpirada, respuesta.Ids);
        Assert.All(respuesta.Anomalias, anomalia => Assert.InRange(anomalia.Severidad, 4, 5));
        Assert.All(respuesta.Anomalias, anomalia => Assert.Null(anomalia.Estado));
        Assert.False(respuesta.TieneETag);
    }

    [Fact]
    public async Task Conductor_que_pide_un_estado_recibe_403()
    {
        // Arrange — escenario 5
        using var mapa = new EscenarioMapa(bd);

        // Act
        var respuesta = await RespuestaMapa.PedirAsync(mapa.Conductor(), "?estado=Expirada");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, respuesta.Http.StatusCode);
    }

    [Fact]
    public async Task Pedir_estado_sin_ser_administrador_es_403_y_con_token_vencido_401()
    {
        // Arrange — escenario 13 (T-SEG20): `estado` es un filtro del panel
        using var mapa = new EscenarioMapa(bd);

        // Act
        var anonimo = await RespuestaMapa.PedirAsync(mapa.Anonimo(), "?estado=Activa");
        var conductor = await RespuestaMapa.PedirAsync(mapa.Conductor(), "?estado=Activa");
        var vencido = await RespuestaMapa.PedirAsync(mapa.ConductorConTokenVencido(), "?estado=Activa");

        // Assert — un token vencido no se trata como anónimo: el cliente debe renovarlo
        Assert.Equal((HttpStatusCode.Forbidden, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized),
            (anonimo.Http.StatusCode, conductor.Http.StatusCode, vencido.Http.StatusCode));
    }
}
