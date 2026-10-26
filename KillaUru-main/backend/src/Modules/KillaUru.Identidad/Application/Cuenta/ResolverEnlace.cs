using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Domain;
namespace KillaUru.Identidad.Application.Cuenta;

/// <summary>Siempre bloquea cuenta antes de leer el enlace: el canje concurrente sigue siendo de un solo uso.</summary>
internal sealed class ResolverEnlace(ITokenCorreoRepositorio tokens, IGeneradorSecretos secretos, IUsuarioRepositorio usuarios)
{
    internal async Task<(Usuario? Usuario, TokenCorreo? Enlace)> CargarAsync(string secreto, CancellationToken ct)
    {
        var hash = secretos.Hash(secreto);
        var id = await tokens.UsuarioPorHashAsync(hash, ct);
        var usuario = id is null ? null : await usuarios.PorIdAsync(id.Value, ct);
        return (usuario, usuario is null ? null : await tokens.PorHashAsync(hash, ct));
    }
}
