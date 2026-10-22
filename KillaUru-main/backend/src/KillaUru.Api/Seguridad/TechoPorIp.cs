using System.Threading.RateLimiting;
using KillaUru.Api.Configuracion;
using Microsoft.AspNetCore.RateLimiting;

namespace KillaUru.Api.Seguridad;

/// <summary>
/// Techo por IP para anónimos (D2): aunque un script cambie de seudónimo en cada petición, una misma
/// IP no pasa de 600 lotes/h ni de 300 consultas/h. Se encadena a la cuota por seudónimo + IP.
/// Los valores dejan margen a redes CGNAT, donde muchos celulares reales comparten IP.
/// </summary>
internal static class TechoPorIp
{
    internal static PartitionedRateLimiter<HttpContext> Crear(OpcionesLimiteTasa limites) =>
        PartitionedRateLimiter.Create<HttpContext, string>(contexto =>
        {
            if (ClaveLimiteTasa.Cuenta(contexto) is not null)
            {
                return RateLimitPartition.GetNoLimiter("con-cuenta");
            }

            var ip = ClaveLimiteTasa.Ip(contexto);
            return PoliticaDe(contexto) switch
            {
                LimiteTasaExtensions.Lotes => LimiteTasaExtensions.PorHora($"ip-lotes|{ip}", limites.TechoIpLotesPorHora),
                LimiteTasaExtensions.Consultas => LimiteTasaExtensions.PorHora($"ip-consultas|{ip}", limites.TechoIpConsultasPorHora),
                _ => RateLimitPartition.GetNoLimiter("sin-techo"),
            };
        });

    private static string? PoliticaDe(HttpContext contexto) =>
        contexto.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName;
}
