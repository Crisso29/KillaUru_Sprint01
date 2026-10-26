using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Domain;
using KillaUru.Identidad.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>
/// La cuenta y su auditoría se confirman en la misma transacción (ADR-013): o quedan ambas o
/// ninguna, y los efectos externos (correo) solo corren tras confirmar. El esquema no guarda
/// nada que vincule cuentas con eventos. Spec 003 · FR-029, FR-038.
/// </summary>
public sealed class PersistenciaTests(BaseDatosPrueba bd)
{
    /// <summary>Lo que hace un caso de uso: escribe en Identidad, audita y agenda un efecto.</summary>
    private sealed class OperacionDePrueba(IServiceProvider servicios, Usuario usuario, bool fallar)
    {
        internal bool EfectoEjecutado { get; private set; }

        internal async Task<int> EjecutarAsync()
        {
            var db = servicios.GetRequiredService<IdentidadDbContext>();
            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync(EscenarioIdentidad.Ct);
            await servicios.GetRequiredService<IAuditoriaService>().RegistrarAsync(AccionAuditada.UsuarioCreado,
                ActorAuditoria.Sistema, usuario.Id.ToString(), new Dictionary<string, object?>(), EscenarioIdentidad.Ct);
            servicios.GetRequiredService<ITransaccionIdentidad>().AlConfirmar(() => { EfectoEjecutado = true; return Task.CompletedTask; });
            if (fallar) throw new InvalidOperationException("Fallo simulado después de escribir ambos módulos.");
            return 1;
        }
    }

    private Task<long> AuditoriasDeAsync(Guid usuarioId) =>
        ContarAsync("SELECT count(*) FROM auditoria.registro WHERE objetivo = @id", ("id", usuarioId.ToString()));

    /// <summary>Consulta directa por SQL: verifica lo que quedó en la base, no lo que cree EF Core.</summary>
    private async Task<long> ContarAsync(string sql, params (string Nombre, object Valor)[] parametros)
    {
        await using var conexion = new Npgsql.NpgsqlConnection(bd.CadenaApp);
        await conexion.OpenAsync(EscenarioIdentidad.Ct);
        await using var comando = new Npgsql.NpgsqlCommand(sql, conexion);
        foreach (var (nombre, valor) in parametros) comando.Parameters.AddWithValue(nombre, valor);
        return (long)(await comando.ExecuteScalarAsync(EscenarioIdentidad.Ct))!;
    }

    private static Usuario UsuarioNuevo(EscenarioIdentidad escenario) =>
        Usuario.Invitar(Correo.Crear(EscenarioIdentidad.CorreoNuevo())!, RolUsuario.Administrador, escenario.Reloj.GetUtcNow());

    /// <summary>Al confirmar quedan la cuenta y su auditoría, y recién entonces corre el efecto.</summary>
    [Fact]
    public async Task Confirmar_guarda_cuenta_y_auditoria_y_luego_el_efecto()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var usuario = UsuarioNuevo(escenario);
        var operacion = new OperacionDePrueba(scope.ServiceProvider, usuario, fallar: false);

        // Act
        await scope.ServiceProvider.GetRequiredService<ITransaccionIdentidad>().EjecutarAsync(operacion.EjecutarAsync, EscenarioIdentidad.Ct);

        // Assert
        var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
        db.ChangeTracker.Clear();
        Assert.True(await db.Usuarios.AnyAsync(x => x.Id == usuario.Id, EscenarioIdentidad.Ct));
        Assert.Equal(1L, await AuditoriasDeAsync(usuario.Id));
        Assert.True(operacion.EfectoEjecutado);
    }

    /// <summary>Si algo falla después de escribir, se deshacen cuenta y auditoría y no hay efecto.</summary>
    [Fact]
    public async Task Fallo_deshace_cuenta_y_auditoria_sin_efecto()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var usuario = UsuarioNuevo(escenario);
        var operacion = new OperacionDePrueba(scope.ServiceProvider, usuario, fallar: true);
        var transaccion = scope.ServiceProvider.GetRequiredService<ITransaccionIdentidad>();

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => transaccion.EjecutarAsync(operacion.EjecutarAsync, EscenarioIdentidad.Ct));
        var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
        db.ChangeTracker.Clear();
        Assert.False(await db.Usuarios.AnyAsync(x => x.Id == usuario.Id, EscenarioIdentidad.Ct));
        Assert.Equal(0L, await AuditoriasDeAsync(usuario.Id));
        Assert.False(operacion.EfectoEjecutado);
    }

    /// <summary>Auditar fuera de una transacción es un error de programación, no un registro suelto (FR-038).</summary>
    [Fact]
    public async Task Sin_transaccion_no_se_puede_auditar()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var auditoria = scope.ServiceProvider.GetRequiredService<IAuditoriaService>();

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => auditoria.RegistrarAsync(AccionAuditada.UsuarioCreado,
            ActorAuditoria.Sistema, null, new Dictionary<string, object?>(), EscenarioIdentidad.Ct));
    }

    /// <summary>
    /// Ninguna columna de Identidad guarda seudónimos, eventos ni ubicaciones (FR-029), y existen
    /// los índices que hacen rápidas la purga y la eliminación en cascada.
    /// </summary>
    [Fact]
    public async Task Esquema_sin_vinculos_a_eventos_y_con_indices_de_cascada()
    {
        // Arrange
        const string ColumnasProhibidas = """
            SELECT count(*) FROM information_schema.columns
            WHERE table_schema = 'identidad' AND column_name ~ '(seudonimo|evento|posicion|lat|lon|ubicacion)'
            """;
        const string IndicesDeCascada = """
            SELECT count(*) FROM pg_indexes WHERE schemaname = 'identidad'
            AND indexname IN ('ix_sesion_usuario_fk', 'ix_token_usuario_fk', 'ix_sesion_expira')
            """;

        // Act
        var columnasProhibidas = await ContarAsync(ColumnasProhibidas);
        var indicesEsperados = await ContarAsync(IndicesDeCascada);

        // Assert
        Assert.Equal(0L, columnasProhibidas);
        Assert.Equal(3L, indicesEsperados);
    }
}
