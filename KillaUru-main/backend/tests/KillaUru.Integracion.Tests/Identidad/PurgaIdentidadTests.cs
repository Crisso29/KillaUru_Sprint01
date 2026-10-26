using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Domain;
using KillaUru.Identidad.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Identidad;

public sealed class PurgaIdentidadTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Purga_respeta_30_31_dias_y_119_121_segundos()
    {
        using var e = new EscenarioIdentidad(bd);
        await using var scope = e.Api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
        var ahora = HorasGuardadas.ALaHora(e.Reloj.GetUtcNow());
        var u = Usuario.Invitar(Correo.Crear(Guid.NewGuid() + "@example.com")!, RolUsuario.ConductorRegistrado, ahora);
        db.Usuarios.Add(u);
        var conservar = Nueva(u.Id, ahora, ahora.AddDays(-30));
        var vencida = Nueva(u.Id, ahora, ahora.AddDays(-31));
        var revocada = Nueva(u.Id, ahora, ahora.AddDays(1));
        revocada.Revocar(ahora.AddDays(-31));
        var exacta = Nueva(u.Id, ahora, ahora.AddDays(1));
        exacta.Usar(ahora.AddSeconds(-119));
        var truncada = Nueva(u.Id, ahora, ahora.AddDays(1));
        truncada.Usar(ahora.AddSeconds(-121));
        db.Sesiones.AddRange(conservar, vencida, revocada, exacta, truncada);
        db.Tokens.Add(TokenCorreo.Crear(Guid.NewGuid().ToByteArray(), u.Id, PropositoToken.Verificacion, ahora.AddDays(-32), TimeSpan.FromDays(1)));
        var usado = TokenCorreo.Crear(Guid.NewGuid().ToByteArray(), u.Id, PropositoToken.Recuperacion, ahora.AddDays(-40), TimeSpan.FromDays(100));
        usado.Usar(ahora.AddDays(-31)); db.Tokens.Add(usado);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        await scope.ServiceProvider.GetRequiredService<IPurgaIdentidadRepositorio>().PurgarAsync(ahora, 30, 2, TestContext.Current.CancellationToken);
        db.ChangeTracker.Clear();
        var filas = await db.Sesiones.Where(x => x.UsuarioId == u.Id).ToDictionaryAsync(x => x.Id, TestContext.Current.CancellationToken);
        Assert.Equal(3, filas.Count);
        Assert.Contains(conservar.Id, filas.Keys);
        Assert.Equal(ahora.AddSeconds(-119), filas[exacta.Id].UsadoUtc);
        Assert.Equal(HorasGuardadas.ALaHora(ahora.AddSeconds(-121)), filas[truncada.Id].UsadoUtc);
        Assert.Equal(0, await db.Tokens.CountAsync(x => x.UsuarioId == u.Id, TestContext.Current.CancellationToken));
    }

    private static SesionRenovacion Nueva(Guid usuario, DateTimeOffset ahora, DateTimeOffset vence) =>
        SesionRenovacion.Crear(usuario, Guid.NewGuid(), Guid.NewGuid().ToByteArray(), ahora.AddDays(-40), vence, null);
}
