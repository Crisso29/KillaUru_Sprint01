using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Mapa;

/// <summary>
/// T021 — el anónimo ve solo el tramo demo y el conductor todo el corredor, con HTTP y PostgreSQL reales.
/// Escenarios 1 y 2 del quickstart; `bbox` sin el demo → vacío; nada de lo que ve el anónimo queda fuera del
/// polígono. Spec 006 · FR-003, FR-004 · SC-003, EQ-11, RNF-27.
/// </summary>
[Collection("Anomalias")]
public sealed class RecorteRolApiTests(BaseDatosPrueba bd)
{
    // Envolvente de la vía sintética: incluye el demo de prueba y lo que queda fuera de él
    private const string BboxCorredor = "?bbox=-75.70,-13.05,-75.10,-12.95";

    [Fact]
    public async Task Anonimo_ve_solo_lo_del_demo_con_cache_publica_y_ETag()
    {
        // Arrange — escenario 1
        using var mapa = new EscenarioMapa(bd);
        var dentro = await mapa.CrearAsync(EscenarioMapa.LongitudDentro());
        var fuera = await mapa.CrearAsync(EscenarioMapa.LongitudFuera());

        // Act
        var respuesta = await RespuestaMapa.PedirAsync(mapa.Anonimo(), BboxCorredor);

        // Assert — el borde (Cloudflare) puede guardarla 60 s porque es igual para todo anónimo (ADR-006)
        Assert.Contains(dentro, respuesta.Ids);
        Assert.DoesNotContain(fuera, respuesta.Ids);
        Assert.True(respuesta.EsPublicaPor60s);
        Assert.True(respuesta.TieneETag);
    }

    [Fact]
    public async Task Conductor_ve_todo_el_corredor_y_despues_el_anonimo_sigue_viendo_solo_el_demo()
    {
        // Arrange — escenario 2: la caché por celdas no debe filtrar la vista del conductor al anónimo (EQ-11)
        using var mapa = new EscenarioMapa(bd);
        var dentro = await mapa.CrearAsync(EscenarioMapa.LongitudDentro());
        var fuera = await mapa.CrearAsync(EscenarioMapa.LongitudFuera());

        // Act
        var conductor = await RespuestaMapa.PedirAsync(mapa.Conductor(), BboxCorredor);
        var anonimoDespues = await RespuestaMapa.PedirAsync(mapa.Anonimo(), BboxCorredor);

        // Assert
        Assert.Contains(dentro, conductor.Ids);
        Assert.Contains(fuera, conductor.Ids);
        Assert.True(conductor.EsPrivadaSinCache);
        Assert.DoesNotContain(fuera, anonimoDespues.Ids);
    }

    [Fact]
    public async Task Anonimo_con_un_bbox_que_no_toca_el_demo_recibe_una_coleccion_vacia()
    {
        // Arrange — un tramo real del corredor, pero lejos del demo
        using var mapa = new EscenarioMapa(bd);
        var fuera = await mapa.CrearAsync(EscenarioMapa.LongitudFuera());

        // Act
        var respuesta = await RespuestaMapa.PedirAsync(mapa.Anonimo(), "?bbox=-75.38,-13.05,-75.10,-12.95");

        // Assert — 200 con «features: []», no 403: el anónimo no puede deducir que ahí hay datos
        Assert.Equal(System.Net.HttpStatusCode.OK, respuesta.Http.StatusCode);
        Assert.Empty(respuesta.Anomalias);
        Assert.NotEqual(Guid.Empty, fuera);
    }

    [Fact]
    public async Task Todo_lo_que_ve_el_anonimo_esta_dentro_del_poligono_demo()
    {
        // Arrange — RNF-27: el 100 %, también cuando la celda solo toca el borde del demo
        using var mapa = new EscenarioMapa(bd);
        await mapa.CrearAsync(EscenarioMapa.OesteDemo + 0.0005);   // celda de borde, punto dentro
        await mapa.CrearAsync(EscenarioMapa.OesteDemo - 0.0005);   // celda de borde, punto fuera
        await mapa.CrearAsync(EscenarioMapa.LongitudFuera());

        // Act — sin bbox: todo el alcance del anónimo
        var respuesta = await RespuestaMapa.PedirAsync(mapa.Anonimo());

        // Assert
        Assert.NotEmpty(respuesta.Anomalias);
        Assert.All(respuesta.Anomalias, anomalia => Assert.True(
            anomalia.Longitud is >= EscenarioMapa.OesteDemo and <= EscenarioMapa.EsteDemo
            && anomalia.Latitud is >= EscenarioMapa.SurDemo and <= EscenarioMapa.NorteDemo,
            $"{anomalia.Id} en ({anomalia.Longitud}, {anomalia.Latitud}) está fuera del demo"));
    }
}
