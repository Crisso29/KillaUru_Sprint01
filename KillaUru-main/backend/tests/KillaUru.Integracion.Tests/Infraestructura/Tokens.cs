using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace KillaUru.Integracion.Tests.Infraestructura;

/// <summary>
/// Emite tokens de prueba con la misma forma que los de Identidad (003): <c>sub</c>, <c>rol</c>,
/// emisor y audiencia del host. Permite fabricar los casos límite (vencido hace 1 s, mal firmado, rol raro).
/// </summary>
internal static class Tokens
{
    internal const string Clave = "clave-de-pruebas-de-integracion-con-32-bytes-o-mas";

    internal static string Emitir(
        string rol, string? sujeto = null, TimeSpan? venceEn = null, string clave = Clave, TimeProvider? reloj = null)
    {
        var ahora = (reloj ?? TimeProvider.System).GetUtcNow().UtcDateTime;
        var vence = ahora + (venceEn ?? TimeSpan.FromMinutes(5));
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "https://api.killauru.pe",
            Audience = "killauru",
            Subject = new ClaimsIdentity([new Claim("sub", sujeto ?? Guid.NewGuid().ToString()), new Claim("rol", rol)]),
            NotBefore = vence.AddMinutes(-10),
            IssuedAt = vence.AddMinutes(-10),
            Expires = vence,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)), SecurityAlgorithms.HmacSha256),
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
