using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Application.Puertos;

/// <summary>Rotación y revocación de familias, manteniendo solo hashes persistidos.</summary>
internal interface ISesionRepositorio
{
    Task<Guid?> UsuarioPorHashAsync(byte[] hash, CancellationToken ct);
    Task<Guid?> UsuarioPorFamiliaAsync(Guid familia, CancellationToken ct);
    Task<SesionRenovacion?> PorHashAsync(byte[] hash, CancellationToken ct);
    void Agregar(SesionRenovacion sesion);
    Task RevocarFamiliaAsync(Guid familia, DateTimeOffset ahora, CancellationToken ct);
    Task RevocarUsuarioAsync(Guid usuario, DateTimeOffset ahora, CancellationToken ct);
}
