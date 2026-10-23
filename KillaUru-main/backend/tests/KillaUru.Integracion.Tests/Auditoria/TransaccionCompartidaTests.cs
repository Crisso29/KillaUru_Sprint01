using KillaUru.Auditoria.Contracts;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Integracion.Tests.Migraciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;
namespace KillaUru.Integracion.Tests.Auditoria;

/// <summary>Prueba ADR-013 con dos contextos y una misma conexión/transacción PostgreSQL.</summary>
public sealed class TransaccionCompartidaTests(BaseDatosPrueba bd)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Accion_y_registro_tienen_el_mismo_desenlace(bool fallaAuditoria)
    {
        await Consulta.EjecutarAsync(bd.CadenaMigrador, "CREATE TABLE IF NOT EXISTS dashboard.accion_prueba (id text PRIMARY KEY)");
        using var escenario = new EscenarioAuditoria(bd.CadenaApp);
        await using var db = new DbContext(new DbContextOptionsBuilder().UseNpgsql(bd.CadenaApp).Options);
        await using var tx = await db.Database.BeginTransactionAsync(Ct);
        escenario.Transaccion.Unir(db.Database.GetDbConnection(), tx.GetDbTransaction());
        var id = Guid.NewGuid().ToString();
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO dashboard.accion_prueba VALUES ({id})", Ct);
        await AuditarYTerminarAsync(escenario, id, fallaAuditoria);
        Assert.Equal(fallaAuditoria ? 0 : 1, await DatosAuditoria.ContarAsync(bd.CadenaApp, id));
        var acciones = await Consulta.TextosAsync(bd.CadenaApp, "SELECT id FROM dashboard.accion_prueba");
        Assert.Equal(!fallaAuditoria, acciones.Contains(id));
    }

    private static async Task AuditarYTerminarAsync(EscenarioAuditoria escenario, string id, bool falla)
    {
        var contexto = new Dictionary<string, object?>();
        if (falla) contexto["correo"] = "prohibido";
        Task Registrar() => escenario.Servicio.RegistrarAsync(AccionAuditada.UsuarioCreado, ActorAuditoria.Sistema, id, contexto, Ct);
        if (falla)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(Registrar);
            await escenario.Transaccion.RevertirAsync(Ct);
        }
        else
        {
            await Registrar();
            await escenario.Transaccion.ConfirmarAsync(Ct);
        }
    }

    [Fact]
    public async Task Sin_transaccion_unida_lanza_y_no_escribe()
    {
        using var escenario = new EscenarioAuditoria(bd.CadenaApp);
        var id = Guid.NewGuid().ToString();
        await Assert.ThrowsAsync<InvalidOperationException>(() => escenario.Servicio.RegistrarAsync(
            AccionAuditada.UsuarioCreado, ActorAuditoria.Sistema, id, new Dictionary<string, object?>(), Ct));
        Assert.Equal(0, await DatosAuditoria.ContarAsync(bd.CadenaApp, id));
    }

    [Fact]
    public async Task Lectura_usa_transaccion_propia_y_confirma()
    {
        using var escenario = new EscenarioAuditoria(bd.CadenaApp);
        var id = Guid.NewGuid().ToString();
        await escenario.Servicio.RegistrarLecturaAsync(AccionAuditada.DatasetExportado,
            ActorAuditoria.Sistema, id, new Dictionary<string, object?> { ["filas"] = 4 }, Ct);
        Assert.Equal(1, await DatosAuditoria.ContarAsync(bd.CadenaApp, id));
        escenario.Metricas.Received(1).RegistrarAccionRiesgo("DatasetExportado");
        Assert.Null(escenario.Transaccion.Transaccion);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Metrica_de_riesgo_solo_tras_commit(bool confirmar)
    {
        using var escenario = new EscenarioAuditoria(bd.CadenaApp);
        await using var db = new DbContext(new DbContextOptionsBuilder().UseNpgsql(bd.CadenaApp).Options);
        await using var tx = await db.Database.BeginTransactionAsync(Ct);
        escenario.Transaccion.Unir(db.Database.GetDbConnection(), tx.GetDbTransaction());
        var id = Guid.NewGuid().ToString();
        await escenario.Servicio.RegistrarAsync(AccionAuditada.RolCambiado,
            ActorAuditoria.Sistema, id, new Dictionary<string, object?>(), Ct);
        escenario.Metricas.DidNotReceive().RegistrarAccionRiesgo(Arg.Any<string>());
        if (confirmar) await escenario.Transaccion.ConfirmarAsync(Ct);
        else await escenario.Transaccion.RevertirAsync(Ct);
        escenario.Metricas.Received(confirmar ? 1 : 0).RegistrarAccionRiesgo("RolCambiado");
        Assert.Equal(confirmar ? 1 : 0, await DatosAuditoria.ContarAsync(bd.CadenaApp, id));
    }

    [Fact]
    public async Task Todas_las_acciones_del_catalogo_se_persisten()
    {
        using var escenario = new EscenarioAuditoria(bd.CadenaApp);
        await using var db = new DbContext(new DbContextOptionsBuilder().UseNpgsql(bd.CadenaApp).Options);
        await using var tx = await db.Database.BeginTransactionAsync(Ct);
        escenario.Transaccion.Unir(db.Database.GetDbConnection(), tx.GetDbTransaction());
        var id = Guid.NewGuid().ToString();
        foreach (var accion in Enum.GetValues<AccionAuditada>())
            await escenario.Servicio.RegistrarAsync(accion, ActorAuditoria.Sistema, id, new Dictionary<string, object?>(), Ct);
        await escenario.Transaccion.ConfirmarAsync(Ct);
        Assert.Equal(14, await DatosAuditoria.ContarAsync(bd.CadenaApp, id));
    }
}
