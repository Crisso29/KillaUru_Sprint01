using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Application.Puertos;

/// <summary>Enlaces de un único propósito y un único uso.</summary>
internal interface ITokenCorreoRepositorio
{
    Task<Guid?> UsuarioPorHashAsync(byte[] hash, CancellationToken ct);
    Task<TokenCorreo?> PorHashAsync(byte[] hash, CancellationToken ct);
    Task InvalidarAsync(Guid usuario, PropositoToken proposito, DateTimeOffset ahora, CancellationToken ct);
    void Agregar(TokenCorreo token);
}
