using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Domain;
using Microsoft.EntityFrameworkCore;

namespace KillaUru.Identidad.Infrastructure.Repositorios;

/// <summary>Invalidación de enlaces anteriores en la misma transacción que su reemplazo.</summary>
internal sealed class TokenCorreoRepositorio(IdentidadDbContext db) : ITokenCorreoRepositorio
{
    public Task<Guid?> UsuarioPorHashAsync(byte[] hash, CancellationToken ct) =>
        db.Tokens.Where(x => x.HashToken == hash).Select(x => (Guid?)x.UsuarioId).SingleOrDefaultAsync(ct);
    public Task<TokenCorreo?> PorHashAsync(byte[] hash, CancellationToken ct) =>
        db.Tokens.SingleOrDefaultAsync(x => x.HashToken == hash, ct);
    public void Agregar(TokenCorreo token) => db.Tokens.Add(token);
    public async Task InvalidarAsync(Guid usuario, PropositoToken proposito, DateTimeOffset ahora, CancellationToken ct) =>
        await db.Tokens.Where(x => x.UsuarioId == usuario && x.Proposito == proposito && x.UsadoUtc == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.UsadoUtc, ahora), ct);
}
