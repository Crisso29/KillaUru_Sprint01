using System.Net;
using System.Net.Http.Json;
using KillaUru.Identidad.Contracts;
using KillaUru.Identidad.Domain;
using KillaUru.Identidad.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>
/// Conteo de conductores activos para el dashboard y derechos del titular: aceptar versiones nuevas
/// y eliminar su cuenta sin dejar rastro de su correo. Spec 003 · FR-004, FR-027, FR-036.
/// </summary>
public sealed class ConsultasYPrivacidadTests(BaseDatosPrueba bd)
{
    /// <summary>
    /// Cuenta conductores distintos con sesión usada en los últimos N días; ignora administradores
    /// y no duplica a quien tiene varias sesiones (FR-036).
    /// </summary>
    [Fact]
    public async Task Dashboard_cuenta_conductores_distintos_dentro_de_la_ventana()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        escenario.Reloj.Advance(TimeSpan.FromDays(3650)); // aísla estos datos de otras pruebas en la misma base
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        await SembrarActividadAsync(scope.ServiceProvider.GetRequiredService<IdentidadDbContext>(), escenario.Reloj.GetUtcNow());
        var consultas = scope.ServiceProvider.GetRequiredService<IIdentidadConsultas>();

        // Act
        var enSieteDias = await consultas.ContarConductoresActivosAsync(7, EscenarioIdentidad.Ct);
        var enNueveDias = await consultas.ContarConductoresActivosAsync(9, EscenarioIdentidad.Ct);

        // Assert
        Assert.Equal(1, enSieteDias);
        Assert.Equal(2, enNueveDias);
    }

    /// <summary>
    /// Un conductor con dos sesiones recientes (1 y 2 días), otro con una de hace 8 días y un
    /// administrador reciente que no debe contarse.
    /// </summary>
    private static async Task SembrarActividadAsync(IdentidadDbContext db, DateTimeOffset ahora)
    {
        var reciente = Usuario.RegistrarConductor(Correo.Crear(EscenarioIdentidad.CorreoNuevo())!, "hash-sintetico", ahora);
        var antiguo = Usuario.RegistrarConductor(Correo.Crear(EscenarioIdentidad.CorreoNuevo())!, "hash-sintetico", ahora);
        var admin = Usuario.Invitar(Correo.Crear(EscenarioIdentidad.CorreoNuevo())!, RolUsuario.Administrador, ahora);
        db.Usuarios.AddRange(reciente, antiguo, admin);
        foreach (var (usuario, diasAtras) in new[] { (reciente, 1), (reciente, 2), (antiguo, 8), (admin, 1) })
            db.Sesiones.Add(SesionRenovacion.Crear(usuario.Id, Guid.NewGuid(), Guid.NewGuid().ToByteArray(), ahora.AddDays(-diasAtras), ahora.AddDays(30), null));
        await db.SaveChangesAsync(EscenarioIdentidad.Ct);
    }

    /// <summary>Aceptar la misma versión dos veces no falla: la app puede reintentar sin miedo (FR-004).</summary>
    [Fact]
    public async Task Aceptar_la_misma_version_dos_veces_es_idempotente()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        escenario.Autenticar(await escenario.RegistrarAsync(EscenarioIdentidad.CorreoNuevo()));
        var aceptacion = new { documento = "PoliticaPrivacidad", version = "1.2" };

        // Act
        var primera = await escenario.Cliente.PostAsJsonAsync("/api/v1/me/aceptaciones", aceptacion, EscenarioIdentidad.Ct);
        var segunda = await escenario.Cliente.PostAsJsonAsync("/api/v1/me/aceptaciones", aceptacion, EscenarioIdentidad.Ct);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, primera.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, segunda.StatusCode);
    }

    /// <summary>Eliminar la cuenta exige la contraseña: con una incorrecta, 403 (FR-027).</summary>
    [Fact]
    public async Task Eliminar_con_contrasena_incorrecta_se_rechaza()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        escenario.Autenticar(await escenario.RegistrarAsync(EscenarioIdentidad.CorreoNuevo()));

        // Act
        var respuesta = await escenario.EliminarMiCuentaAsync("incorrecta");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        Assert.Equal("confirmacion_invalida", await EscenarioIdentidad.CodigoAsync(respuesta));
    }

    /// <summary>
    /// Eliminar borra físicamente cuenta, sesiones, enlaces y consentimientos; la auditoría no
    /// conserva el correo y el token deja de servir (FR-027).
    /// </summary>
    [Fact]
    public async Task Eliminar_borra_los_datos_sin_dejar_el_correo_en_auditoria()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var correo = EscenarioIdentidad.CorreoNuevo();
        var sesion = await escenario.RegistrarAsync(correo);
        var id = EscenarioIdentidad.UsuarioId(sesion);
        escenario.Autenticar(sesion);

        // Act
        var respuesta = await escenario.EliminarMiCuentaAsync(EscenarioIdentidad.Contrasena);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await escenario.MisDatosAsync()).StatusCode);
        Assert.False(await QuedanDatosDeAsync(escenario, id));
        Assert.Equal(0L, await RegistrosDeAuditoriaConAsync(correo));
    }

    /// <summary>Busca al usuario en las cuatro tablas que guardan algo suyo.</summary>
    private static async Task<bool> QuedanDatosDeAsync(EscenarioIdentidad escenario, Guid id)
    {
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
        return await db.Usuarios.AnyAsync(x => x.Id == id, EscenarioIdentidad.Ct)
            || await db.Sesiones.AnyAsync(x => x.UsuarioId == id, EscenarioIdentidad.Ct)
            || await db.Tokens.AnyAsync(x => x.UsuarioId == id, EscenarioIdentidad.Ct)
            || await db.Consentimientos.AnyAsync(x => x.UsuarioId == id, EscenarioIdentidad.Ct);
    }

    private async Task<long> RegistrosDeAuditoriaConAsync(string texto)
    {
        await using var conexion = new Npgsql.NpgsqlConnection(bd.CadenaApp);
        await conexion.OpenAsync(EscenarioIdentidad.Ct);
        await using var comando = new Npgsql.NpgsqlCommand("SELECT count(*) FROM auditoria.registro WHERE row_to_json(registro)::text LIKE @texto", conexion);
        comando.Parameters.AddWithValue("texto", "%" + texto + "%");
        return (long)(await comando.ExecuteScalarAsync(EscenarioIdentidad.Ct))!;
    }
}
