using System.Security.Cryptography;
using System.Text;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Puertos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace KillaUru.Identidad.Infrastructure.Seguridad;

/// <summary>Derivación HMAC reproducible sin almacenar secretos en claro (R-003).</summary>
internal sealed class GeneradorSecretos(IOptions<IdentidadOptions> opciones) : IGeneradorSecretos
{
    public string Aleatorio32() => Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
    public byte[] Hash(string secreto) => SHA256.HashData(Encoding.UTF8.GetBytes(secreto));
    public string Derivar(string anterior) => Base64UrlEncoder.Encode(
        HMACSHA256.HashData(Encoding.UTF8.GetBytes(opciones.Value.ClaveRotacion), Encoding.UTF8.GetBytes(anterior)));
}
