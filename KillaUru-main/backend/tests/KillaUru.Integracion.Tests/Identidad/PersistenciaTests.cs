using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Domain;
using KillaUru.Identidad.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Metricas;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Identidad;

public sealed class PersistenciaTests(BaseDatosPrueba bd)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Cuenta_y_auditoria_se_confirman_juntas_o_ninguna(bool fallar)
    {
        using var e = new EscenarioIdentidad(bd);
        await using var scope = e.Api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
        var tx = scope.ServiceProvider.GetRequiredService<ITransaccionIdentidad>();
        var auditoria = scope.ServiceProvider.GetRequiredService<IAuditoriaService>();
        var u = Usuario.Invitar(Correo.Crear(Guid.NewGuid() + "@example.com")!, RolUsuario.Administrador, e.Reloj.GetUtcNow());
        var confirmado = false;
        async Task<int> Accion()
        {
            db.Usuarios.Add(u);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            await auditoria.RegistrarAsync(AccionAuditada.UsuarioCreado, ActorAuditoria.Sistema, u.Id.ToString(),
                new Dictionary<string, object?>(), TestContext.Current.CancellationToken);
            tx.AlConfirmar(() => { confirmado = true; return Task.CompletedTask; });
            Assert.False(confirmado);
            if (fallar) throw new InvalidOperationException("Fallo simulado después de escribir ambos módulos.");
            return 1;
        }
        if (fallar) await Assert.ThrowsAsync<InvalidOperationException>(() => tx.EjecutarAsync(Accion, TestContext.Current.CancellationToken));
        else await tx.EjecutarAsync(Accion, TestContext.Current.CancellationToken);
        db.ChangeTracker.Clear();
        Assert.Equal(!fallar, await db.Usuarios.AnyAsync(x => x.Id == u.Id, TestContext.Current.CancellationToken));
        await using var conexion = new Npgsql.NpgsqlConnection(bd.CadenaApp);
        await conexion.OpenAsync(TestContext.Current.CancellationToken);
        await using var comando = new Npgsql.NpgsqlCommand("SELECT count(*) FROM auditoria.registro WHERE objetivo = @id", conexion);
        comando.Parameters.AddWithValue("id", u.Id.ToString());
        Assert.Equal(fallar ? 0L : 1L, await comando.ExecuteScalarAsync(TestContext.Current.CancellationToken));
        Assert.Equal(!fallar, confirmado);
    }

    [Fact]
    public async Task Sin_transaccion_no_se_puede_escribir_auditoria()
    {
        using var e = new EscenarioIdentidad(bd);
        await using var scope = e.Api.Services.CreateAsyncScope();
        var auditoria = scope.ServiceProvider.GetRequiredService<IAuditoriaService>();
        await Assert.ThrowsAsync<InvalidOperationException>(() => auditoria.RegistrarAsync(AccionAuditada.UsuarioCreado,
            ActorAuditoria.Sistema, null, new Dictionary<string, object?>(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Esquema_sin_vinculos_a_eventos_y_con_indices_de_cascada()
    {
        await using var conexion = new Npgsql.NpgsqlConnection(bd.CadenaApp);
        await conexion.OpenAsync(TestContext.Current.CancellationToken);
        await using var columnas = new Npgsql.NpgsqlCommand("""
            SELECT count(*) FROM information_schema.columns
            WHERE table_schema = 'identidad' AND column_name ~ '(seudonimo|evento|posicion|lat|lon|ubicacion)'
            """, conexion);
        Assert.Equal(0L, await columnas.ExecuteScalarAsync(TestContext.Current.CancellationToken));
        await using var indices = new Npgsql.NpgsqlCommand("""
            SELECT count(*) FROM pg_indexes WHERE schemaname = 'identidad'
            AND indexname IN ('ix_sesion_usuario_fk', 'ix_token_usuario_fk', 'ix_sesion_expira')
            """, conexion);
        Assert.Equal(3L, await indices.ExecuteScalarAsync(TestContext.Current.CancellationToken));
    }
}
