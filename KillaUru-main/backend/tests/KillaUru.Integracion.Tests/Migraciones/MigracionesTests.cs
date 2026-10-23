using KillaUru.Integracion.Tests.Infraestructura;
using Npgsql;

namespace KillaUru.Integracion.Tests.Migraciones;

/// <summary>T022: la base queda como dice data-model §1, con roles separados y migraciones atómicas.</summary>
public sealed class MigracionesTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Existen_los_esquemas_de_modulo_y_el_de_migraciones()
    {
        var esquemas = await Consulta.TextosAsync(bd.CadenaSuperusuario,
            "SELECT nspname FROM pg_namespace WHERE nspowner = 'killauru_migrador'::regrole ORDER BY 1");

        Assert.Equal(["anomalias", "auditoria", "dashboard", "identidad", "ingesta", "migraciones"], esquemas);
    }

    [Fact]
    public async Task Existen_los_roles_y_las_extensiones()
    {
        var roles = await Consulta.TextosAsync(bd.CadenaSuperusuario,
            "SELECT rolname FROM pg_roles WHERE rolname LIKE 'killauru_%' ORDER BY 1");
        var extensiones = await Consulta.TextosAsync(bd.CadenaSuperusuario,
            "SELECT extname FROM pg_extension WHERE extname IN ('postgis', 'citext') ORDER BY 1");

        Assert.Equal(["killauru_app", "killauru_migrador"], roles);
        Assert.Equal(["citext", "postgis"], extensiones);
    }

    [Fact]
    public async Task Se_registran_V0000_y_V0001()
    {
        var versiones = await Consulta.TextosAsync(bd.CadenaApp, "SELECT version FROM migraciones.aplicada ORDER BY 1");

        Assert.Contains("V0000", versiones);
        Assert.Contains("V0001", versiones);
    }

    [Fact]
    public async Task La_app_no_puede_crear_tablas() =>
        await Assert.ThrowsAsync<PostgresException>(() =>
            Consulta.EjecutarAsync(bd.CadenaApp, "CREATE TABLE ingesta.intruso (id int)"));

    [Fact]
    public async Task La_app_lee_y_escribe_lo_que_crea_el_migrador_salvo_auditoria()
    {
        await Consulta.EjecutarAsync(bd.CadenaMigrador, "CREATE TABLE IF NOT EXISTS dashboard.prueba_privilegios (id int)");
        await Consulta.EjecutarAsync(bd.CadenaMigrador, "CREATE TABLE IF NOT EXISTS auditoria.prueba_privilegios (id int)");

        await Consulta.EjecutarAsync(bd.CadenaApp, "INSERT INTO dashboard.prueba_privilegios VALUES (1); DELETE FROM dashboard.prueba_privilegios");
        await Consulta.EjecutarAsync(bd.CadenaApp, "INSERT INTO auditoria.prueba_privilegios VALUES (1)");
        await Assert.ThrowsAsync<PostgresException>(() =>
            Consulta.EjecutarAsync(bd.CadenaApp, "DELETE FROM auditoria.prueba_privilegios"));   // append-only (RNF-31)
    }

    [Fact]
    public async Task Un_script_con_error_no_deja_nada_aplicado()
    {
        var script = Path.Combine(Path.GetTempPath(), "V9999__roto.sql");
        await File.WriteAllTextAsync(script,
            "CREATE TABLE ingesta.a_medias (id int);\nSELECT * FROM tabla_que_no_existe;\n", TestContext.Current.CancellationToken);
        await bd.Migrador.CopiarAsync(script);

        var resultado = await bd.Migrador.AplicarAsync(script, exigirExito: false);
        var tablas = await Consulta.TextosAsync(bd.CadenaSuperusuario,
            "SELECT tablename FROM pg_tables WHERE tablename = 'a_medias'");
        var registro = await Consulta.TextosAsync(bd.CadenaSuperusuario,
            "SELECT version FROM migraciones.aplicada WHERE version = 'V9999'");

        Assert.NotEqual(0, resultado.ExitCode);
        Assert.Empty(tablas);
        Assert.Empty(registro);
    }
}
