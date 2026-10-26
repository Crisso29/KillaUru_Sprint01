using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Domain;
using KillaUru.Identidad.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>
/// La purga diaria borra sesiones vencidas o revocadas hace más de 30 días y enlaces usados o
/// vencidos; además trunca a la hora el uso de sesiones pasados 2 min de gracia (la precisión
/// exacta solo hace falta para la gracia de 60 s). Spec 003 · FR-039, FR-040.
/// </summary>
public sealed class PurgaIdentidadTests(BaseDatosPrueba bd)
{
    private const int RetencionDias = 30;
    private const int TruncarUsoTrasMinutos = 2;

    /// <summary>Sesiones con fechas en cada lado de los límites de 30 días y 120 s.</summary>
    private sealed record Sesiones(SesionRenovacion Vence30Dias, SesionRenovacion Vence31Dias, SesionRenovacion Revocada31Dias,
        SesionRenovacion Usada119Segundos, SesionRenovacion Usada121Segundos)
    {
        internal SesionRenovacion[] Todas => [Vence30Dias, Vence31Dias, Revocada31Dias, Usada119Segundos, Usada121Segundos];
    }

    private static SesionRenovacion Sesion(Guid usuario, DateTimeOffset ahora, DateTimeOffset vence) =>
        SesionRenovacion.Crear(usuario, Guid.NewGuid(), Guid.NewGuid().ToByteArray(), ahora.AddDays(-40), vence, null);

    private static Sesiones CrearSesiones(Guid usuario, DateTimeOffset ahora)
    {
        var revocada = Sesion(usuario, ahora, ahora.AddDays(1));
        revocada.Revocar(ahora.AddDays(-31));
        var usada119 = Sesion(usuario, ahora, ahora.AddDays(1));
        usada119.Usar(ahora.AddSeconds(-119));
        var usada121 = Sesion(usuario, ahora, ahora.AddDays(1));
        usada121.Usar(ahora.AddSeconds(-121));
        return new(Sesion(usuario, ahora, ahora.AddDays(-30)), Sesion(usuario, ahora, ahora.AddDays(-31)), revocada, usada119, usada121);
    }

    private static TokenCorreo[] CrearEnlacesViejos(Guid usuario, DateTimeOffset ahora)
    {
        var vencido = TokenCorreo.Crear(Guid.NewGuid().ToByteArray(), usuario, PropositoToken.Verificacion, ahora.AddDays(-32), TimeSpan.FromDays(1));
        var usado = TokenCorreo.Crear(Guid.NewGuid().ToByteArray(), usuario, PropositoToken.Recuperacion, ahora.AddDays(-40), TimeSpan.FromDays(100));
        usado.Usar(ahora.AddDays(-31));
        return [vencido, usado];
    }

    /// <summary>Lee de nuevo desde la base (sin la caché de EF) lo que dejó la purga.</summary>
    private static async Task<Dictionary<Guid, SesionRenovacion>> SesionesQueQuedanAsync(IdentidadDbContext db, Guid usuarioId)
    {
        db.ChangeTracker.Clear();
        return await db.Sesiones.Where(x => x.UsuarioId == usuarioId).ToDictionaryAsync(x => x.Id, EscenarioIdentidad.Ct);
    }

    private static Task PurgarAsync(IServiceProvider servicios, DateTimeOffset ahora) =>
        servicios.GetRequiredService<IPurgaIdentidadRepositorio>().PurgarAsync(ahora, RetencionDias, TruncarUsoTrasMinutos, EscenarioIdentidad.Ct);

    private static async Task<(Guid UsuarioId, Sesiones Sesiones)> SembrarAsync(IdentidadDbContext db, DateTimeOffset ahora)
    {
        var usuario = Usuario.Invitar(Correo.Crear(EscenarioIdentidad.CorreoNuevo())!, RolUsuario.ConductorRegistrado, ahora);
        var sesiones = CrearSesiones(usuario.Id, ahora);
        db.Usuarios.Add(usuario);
        db.Sesiones.AddRange(sesiones.Todas);
        db.Tokens.AddRange(CrearEnlacesViejos(usuario.Id, ahora));
        await db.SaveChangesAsync(EscenarioIdentidad.Ct);
        return (usuario.Id, sesiones);
    }

    /// <summary>Se conservan las de 30 días; se borran las de 31; el uso a 121 s se trunca y a 119 s no.</summary>
    [Fact]
    public async Task Purga_respeta_los_limites_de_30_dias_y_120_segundos()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
        var ahora = HorasGuardadas.ALaHora(escenario.Reloj.GetUtcNow());
        var (usuarioId, sesiones) = await SembrarAsync(db, ahora);

        // Act
        await PurgarAsync(scope.ServiceProvider, ahora);

        // Assert
        var quedan = await SesionesQueQuedanAsync(db, usuarioId);
        Assert.Equal(3, quedan.Count);
        Assert.Contains(sesiones.Vence30Dias.Id, quedan.Keys);
        Assert.Equal(ahora.AddSeconds(-119), quedan[sesiones.Usada119Segundos.Id].UsadoUtc);
        Assert.Equal(HorasGuardadas.ALaHora(ahora.AddSeconds(-121)), quedan[sesiones.Usada121Segundos.Id].UsadoUtc);
        Assert.Equal(0, await db.Tokens.CountAsync(x => x.UsuarioId == usuarioId, EscenarioIdentidad.Ct));
    }
}
