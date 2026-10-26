using System.Text;
using KillaUru.Api.Configuracion;
using KillaUru.Api.Errores;
using KillaUru.Shared.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace KillaUru.Api.Seguridad;

/// <summary>
/// Valida firma, emisor, audiencia, vigencia y rol del token: Bearer (app) o cookie <c>ku_acceso</c> (panel).
/// No agrega tolerancia de reloj; Identidad debe emitir tokens con duración de 5 min (ADR-009, RNF-26).
/// Sin token, el rol es Anónimo. Emitir tokens es tarea de Identidad (003), no del host.
/// </summary>
internal static class AutenticacionExtensions
{
    internal const string CookieAcceso = "ku_acceso";
    internal const string ClaimSujeto = "sub";
    internal const string ClaimRol = "rol";

    private static readonly HashSet<string> RolesConToken = [Roles.ConductorRegistrado, Roles.Administrador];

    internal static IServiceCollection AgregarAutenticacion(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<ApiOptions>, TimeProvider>((jwt, api, reloj) => Configurar(jwt, api.Value.Jwt, reloj));
        services.AddAuthorization(opciones => opciones.AddPolicy("SoloAdministrador",
            politica => politica.RequireAuthenticatedUser().RequireRole(Roles.Administrador)));
        return services;
    }

    private static void Configurar(JwtBearerOptions jwt, OpcionesJwt opciones, TimeProvider reloj)
    {
        jwt.MapInboundClaims = false;
        jwt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = opciones.Emisor,
            ValidAudience = opciones.Audiencia,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opciones.ClaveFirma)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimSujeto,
            RoleClaimType = ClaimRol,
            LifetimeValidator = (desde, hasta, _, _) => hasta is not null && hasta > reloj.GetUtcNow().UtcDateTime
                && (desde is null || desde <= reloj.GetUtcNow().UtcDateTime),
        };
        jwt.Events = Eventos();
    }

    private static JwtBearerEvents Eventos() => new()
    {
        OnMessageReceived = LeerCookieSiNoHayBearer,
        OnTokenValidated = ExigirRolConocido,
        OnChallenge = contexto =>
        {
            contexto.HandleResponse();
            return RespuestaProblema.EscribirAsync(
                contexto.HttpContext, StatusCodes.Status401Unauthorized, "token_invalido",
                "El token de acceso no es válido o venció.").AsTask();
        },
        OnForbidden = contexto => RespuestaProblema.EscribirAsync(
            contexto.HttpContext, StatusCodes.Status403Forbidden, "sin_permiso",
            "Tu rol no tiene permiso para esta operación.").AsTask(),
    };

    private static Task LeerCookieSiNoHayBearer(MessageReceivedContext contexto)
    {
        // Cualquier Authorization presente tiene prioridad, aunque no contenga un Bearer válido.
        var tieneBearer = contexto.Request.Headers.ContainsKey(HeaderNames.Authorization);
        var esPublico = contexto.Request.Path.StartsWithSegments("/api/v1/auth")
            && contexto.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null;
        if (!tieneBearer && !esPublico && contexto.Request.Cookies.TryGetValue(CookieAcceso, out var token))
        {
            contexto.Token = token;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Rechaza tokens con rol fuera del catálogo o con sujeto vacío.
    /// Un token firmado correctamente pero con rol desconocido indica una emisión incorrecta
    /// que no debe pasar a la capa de autorización (ADR-009).
    /// </summary>
    private static Task ExigirRolConocido(TokenValidatedContext contexto)
    {
        var rol     = contexto.Principal?.FindFirst(ClaimRol)?.Value;
        var subTexto = contexto.Principal?.FindFirst(ClaimSujeto)?.Value;

        var rolValido = rol is not null && RolesConToken.Contains(rol);
        var subValido = Guid.TryParse(subTexto, out var sujeto) && sujeto != Guid.Empty;

        if (!rolValido || !subValido)
        {
            contexto.Fail("Rol desconocido o sujeto inválido en el token.");
        }

        return Task.CompletedTask;
    }
}
