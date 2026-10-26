using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace KillaUru.Identidad.Infrastructure.Seguridad;

/// <summary>JWT mínimo; jti identifica la familia para cerrar la sesión desde su cookie de acceso.</summary>
internal sealed class EmisorJwt(IOptions<OpcionesFirmaJwt> opciones) : IEmisorJwt
{
    public string Emitir(Usuario usuario, Guid familia, DateTimeOffset ahora, DateTimeOffset vence)
    {
        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opciones.Value.ClaveFirma));
        var firma = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);
        // Solo sub, rol y verificado (FR-014): el token viaja en cada petición y no debe llevar datos personales.
        Claim[] claims =
        [
            new("sub", usuario.Id.ToString()), new("rol", usuario.Rol.ToString()),
            new("verificado", usuario.CorreoVerificado ? "true" : "false", ClaimValueTypes.Boolean),
            new("jti", familia.ToString()),
        ];
        var jwt = new JwtSecurityToken(opciones.Value.Emisor, opciones.Value.Audiencia, claims,
            ahora.UtcDateTime, vence.UtcDateTime, firma);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
