using KillaUru.Api.Configuracion;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace KillaUru.Api.Seguridad;

/// <summary>
/// CORS solo para el panel (FR-014): un único origen, con credenciales, porque el panel se autentica
/// con la cookie <c>ku_acceso</c>. La app Android no usa CORS.
/// </summary>
internal static class CorsPanelExtensions
{
    internal static IServiceCollection AgregarCorsPanel(this IServiceCollection services)
    {
        services.AddCors();
        services.AddOptions<CorsOptions>().Configure<IOptions<ApiOptions>>((opciones, api) =>
            opciones.AddDefaultPolicy(politica => politica
                .WithOrigins(api.Value.OrigenPanel)
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod()));
        return services;
    }
}
