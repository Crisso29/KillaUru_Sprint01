using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KillaUru.Identidad.Contracts;
using KillaUru.Identidad.Domain;
using KillaUru.Identidad.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>Comprueba agregación sin duplicados y eliminación física de datos del titular.</summary>
public sealed class ConsultasYPrivacidadTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Dashboard_cuenta_conductores_distintos_dentro_de_la_ventana()
    {
        using var e = new EscenarioIdentidad(bd);
        // Aislar temporalmente el conjunto respecto de otras pruebas con la misma base.
        e.Reloj.Advance(TimeSpan.FromDays(3650));
        await using var scope = e.Api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
        var ahora = e.Reloj.GetUtcNow();
        var conductor = Usuario.RegistrarConductor(Correo.Crear(Guid.NewGuid() + "@example.com")!, "hash-sintetico", ahora);
        var antiguo = Usuario.RegistrarConductor(Correo.Crear(Guid.NewGuid() + "@example.com")!, "hash-sintetico", ahora);
        var admin = Usuario.Invitar(Correo.Crear(Guid.NewGuid() + "@example.com")!, RolUsuario.Administrador, ahora);
        db.Usuarios.AddRange(conductor, antiguo, admin);
        foreach (var (usuario, dias) in new[] { (conductor, 1), (conductor, 2), (antiguo, 8), (admin, 1) })
        {
            db.Sesiones.Add(SesionRenovacion.Crear(usuario.Id, Guid.NewGuid(), Guid.NewGuid().ToByteArray(),
                ahora.AddDays(-dias), ahora.AddDays(30), null));
        }
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var consultas = scope.ServiceProvider.GetRequiredService<IIdentidadConsultas>();
        Assert.Equal(1, await consultas.ContarConductoresActivosAsync(7, TestContext.Current.CancellationToken));
        Assert.Equal(2, await consultas.ContarConductoresActivosAsync(9, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Aceptacion_idempotente_y_eliminacion_borra_dependencias_sin_correo_en_auditoria()
    {
        using var e = new EscenarioIdentidad(bd);
        var correo = Guid.NewGuid() + "@example.com";
        e.Autenticar(await e.RegistrarAsync(correo));
        await using var scope = e.Api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
        var id = await db.Usuarios.Where(x => x.Correo == Correo.Crear(correo)).Select(x => x.Id).SingleAsync(TestContext.Current.CancellationToken);
        for (var i = 0; i < 2; i++)
        {
            var aceptar = await e.Cliente.PostAsJsonAsync("/api/v1/me/aceptaciones", new { documento = "PoliticaPrivacidad", version = "1.2" }, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.NoContent, aceptar.StatusCode);
        }
        using var incorrecta = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/me") { Content = JsonContent.Create(new { contrasena = "incorrecta" }) };
        var rechazo = await e.Cliente.SendAsync(incorrecta, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, rechazo.StatusCode);
        Assert.Contains("confirmacion_invalida", await rechazo.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        using var eliminar = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/me") { Content = JsonContent.Create(new { contrasena = EscenarioIdentidad.Contrasena }) };
        Assert.Equal(HttpStatusCode.NoContent, (await e.Cliente.SendAsync(eliminar, TestContext.Current.CancellationToken)).StatusCode);
        Assert.False(await db.Usuarios.AnyAsync(x => x.Id == id, TestContext.Current.CancellationToken));
        Assert.False(await db.Sesiones.AnyAsync(x => x.UsuarioId == id, TestContext.Current.CancellationToken));
        Assert.False(await db.Tokens.AnyAsync(x => x.UsuarioId == id, TestContext.Current.CancellationToken));
        Assert.False(await db.Consentimientos.AnyAsync(x => x.UsuarioId == id, TestContext.Current.CancellationToken));
        await using var conexion = new Npgsql.NpgsqlConnection(bd.CadenaApp);
        await conexion.OpenAsync(TestContext.Current.CancellationToken);
        await using var comando = new Npgsql.NpgsqlCommand("SELECT count(*) FROM auditoria.registro WHERE row_to_json(registro)::text LIKE @correo", conexion);
        comando.Parameters.AddWithValue("correo", "%" + correo + "%");
        Assert.Equal(0L, await comando.ExecuteScalarAsync(TestContext.Current.CancellationToken));
    }
}
