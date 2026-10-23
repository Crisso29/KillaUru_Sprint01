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
        services.AddOptions<ForwardedHeadersOptions>().Configure<IOptions<ApiOptions>>((opciones, api) =>
        {
            opciones.KnownProxies.Clear();
            opciones.KnownIPNetworks.Clear();

            // Sin red del túnel no se confía en nadie. Ojo: con ambas listas vacías ASP.NET confiaría en
            // CUALQUIER origen, así que en ese caso la cabecera directamente no se procesa.
            if (!System.Net.IPNetwork.TryParse(api.Value.RedTunel, out var red))
            {
                opciones.ForwardedHeaders = ForwardedHeaders.None;
                return;
            }

            opciones.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
            opciones.ForwardedForHeaderName = CabeceraCloudflare;
            opciones.ForwardLimit = 1;
            opciones.KnownIPNetworks.Add(red);
        });
        return services;
    }
}
