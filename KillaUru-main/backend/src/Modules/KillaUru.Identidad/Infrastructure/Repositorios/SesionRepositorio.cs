using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Domain;
using Microsoft.EntityFrameworkCore;

namespace KillaUru.Identidad.Infrastructure.Repositorios;

/// <summary>Revocación por conjuntos; conserva las generaciones para detectar reuso.</summary>
internal sealed class SesionRepositorio(IdentidadDbContext db) : ISesionRepositorio
{
    public Task<Guid?> UsuarioPorHashAsync(byte[] hash, CancellationToken ct) =>
        db.Sesiones.Where(x => x.HashToken == hash).Select(x => (Guid?)x.UsuarioId).SingleOrDefaultAsync(ct);
    public Task<Guid?> UsuarioPorFamiliaAsync(Guid familia, CancellationToken ct) =>
        db.Sesiones.Where(x => x.FamiliaId == familia).Select(x => (Guid?)x.UsuarioId).FirstOrDefaultAsync(ct);
    public Task<SesionRenovacion?> PorHashAsync(byte[] hash, CancellationToken ct) =>
        db.Sesiones.SingleOrDefaultAsync(x => x.HashToken == hash, ct);
    public void Agregar(SesionRenovacion sesion) => db.Sesiones.Add(sesion);
    public async Task RevocarFamiliaAsync(Guid familia, DateTimeOffset ahora, CancellationToken ct) =>
        await db.Sesiones.Where(x => x.FamiliaId == familia && x.RevocadoUtc == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevocadoUtc, HorasGuardadas.ALaHora(ahora)), ct);
    public async Task RevocarUsuarioAsync(Guid usuario, DateTimeOffset ahora, CancellationToken ct) =>
        await db.Sesiones.Where(x => x.UsuarioId == usuario && x.RevocadoUtc == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevocadoUtc, HorasGuardadas.ALaHora(ahora)), ct);
}
