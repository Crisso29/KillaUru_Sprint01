using KillaUru.Api.Configuracion;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace KillaUru.Api.Seguridad;

/// <summary>
/// La IP real llega en CF-Connecting-IP (RNF-11), pero solo se cree si la conexión viene de la red
/// de cloudflared: así nadie puede inventarse otra IP para esquivar los límites (FR-011).
/// </summary>
internal static class IpRealExtensions
{
    private const string CabeceraCloudflare = "CF-Connecting-IP";

    internal static IServiceCollection AgregarIpReal(this IServiceCollection services)
    {
        services.AddOptions<ForwardedHeadersOptions>().Configure<IOptions<ApiOptions>>((opciones, api) => Configurar(opciones, api.Value.RedTunel));
        return services;
    }

    /// <summary>
    /// Configura la resolución de IP real desde la cabecera CF-Connecting-IP.
    /// Solo acepta esa cabecera si la conexión viene de la red del túnel cloudflared (FR-011).
    /// Sin red de túnel configurada, se deshabilita el forwarding completamente: con ambas listas
    /// vacías ASP.NET confiaría en cualquier cliente, lo que permitiría falsificar la IP.
    /// </summary>
    private static void Configurar(ForwardedHeadersOptions opciones, string? redTunel)
    {
        opciones.KnownProxies.Clear();
        opciones.KnownIPNetworks.Clear();

        if (!System.Net.IPNetwork.TryParse(redTunel, out var red))
        {
            // Sin red del túnel válida: ignorar CF-Connecting-IP en todos los casos (FR-011)
            opciones.ForwardedHeaders = ForwardedHeaders.None;
            return;
        }

        opciones.ForwardedHeaders    = ForwardedHeaders.XForwardedFor;
        opciones.ForwardedForHeaderName = CabeceraCloudflare;
        opciones.ForwardLimit        = 1;       // Solo un salto: cloudflared → API
        opciones.KnownIPNetworks.Add(red);
    }
}
