using System.Globalization;
using System.Threading.RateLimiting;
using KillaUru.Api.Configuracion;
using KillaUru.Api.Errores;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace KillaUru.Api.Seguridad;

/// <summary>
/// Límites de tasa de RNF-11 en dos capas:
/// global = cuota por cuenta (1 000 o 3 000/h) encadenada al techo por IP de los anónimos (D2);
/// por endpoint = cuota del anónimo por seudónimo + IP (<c>consultas</c> 60/h, <c>lotes</c> 120/h)
/// y <c>registro</c> (5/h por IP, D8). Al pasarse: 429 con Retry-After.
/// </summary>
internal static class LimiteTasaExtensions
{
    /// <summary>Política de los GET del mapa y demás consultas.</summary>
    internal const string Consultas = "consultas";

    /// <summary>Política del envío de lotes (cuota aparte, para que una subida acumulada no bloquee el mapa).</summary>
    internal const string Lotes = "lotes";

    /// <summary>Política del registro de cuentas.</summary>
    internal const string Registro = "registro";

    private static readonly TimeSpan Hora = TimeSpan.FromHours(1);

    internal static IServiceCollection AgregarLimiteTasa(this IServiceCollection services)
    {
        services.AddRateLimiter(_ => { });
        services.AddOptions<RateLimiterOptions>()
            .Configure<IOptions<ApiOptions>>((opciones, api) => Configurar(opciones, api.Value.LimiteTasa));
        return services;
    }

    /// <summary>Crea una ventana fija de una hora, sin cola de espera.</summary>
    /// <remarks>
    /// Al renovarse la ventana se recupera toda la cuota. No implementa la ventana deslizante
    /// acordada en research.md R-004; dos ráfagas a ambos lados del corte pueden consumir dos cuotas.
    /// </remarks>
    internal static RateLimitPartition<string> PorHora(string clave, int limite) =>
        RateLimitPartition.GetFixedWindowLimiter(clave, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = limite, Window = Hora, QueueLimit = 0, AutoReplenishment = true,
        });

    private static void Configurar(RateLimiterOptions opciones, OpcionesLimiteTasa limites)
    {
        opciones.GlobalLimiter = PartitionedRateLimiter.CreateChained(PorCuenta(limites), TechoPorIp.Crear(limites));
        opciones.AddPolicy(Consultas, contexto => SoloAnonimo(contexto, limites.AnonimoConsultasPorHora));
        opciones.AddPolicy(Lotes, contexto => SoloAnonimo(contexto, limites.AnonimoLotesPorHora));
        opciones.AddPolicy(Registro, contexto => PorHora($"registro|{ClaveLimiteTasa.Ip(contexto)}", limites.RegistrosPorHoraPorIp));
        opciones.OnRejected = RechazarAsync;
    }

    private static PartitionedRateLimiter<HttpContext> PorCuenta(OpcionesLimiteTasa limites) =>
        PartitionedRateLimiter.Create<HttpContext, string>(contexto => ClaveLimiteTasa.Cuenta(contexto) is { } cuenta
            ? PorHora($"cuenta|{cuenta}", ClaveLimiteTasa.EsAdministrador(contexto) ? limites.AdministradorPorHora : limites.RegistradoPorHora)
            : RateLimitPartition.GetNoLimiter("anonimo"));

    private static RateLimitPartition<string> SoloAnonimo(HttpContext contexto, int limite) =>
        ClaveLimiteTasa.Cuenta(contexto) is null
            ? PorHora(ClaveLimiteTasa.SeudonimoMasIp(contexto), limite)
            : RateLimitPartition.GetNoLimiter("con-cuenta");

    private static ValueTask RechazarAsync(OnRejectedContext rechazo, CancellationToken ct)
    {
        var segundos = rechazo.Lease.TryGetMetadata(MetadataName.RetryAfter, out var espera)
            ? (int)Math.Ceiling(espera.TotalSeconds)
            : (int)Hora.TotalSeconds;
        rechazo.HttpContext.Response.Headers.RetryAfter = Math.Max(1, segundos).ToString(CultureInfo.InvariantCulture);
        return RespuestaProblema.EscribirAsync(
            rechazo.HttpContext, StatusCodes.Status429TooManyRequests, "limite_excedido", "Superaste el límite de peticiones.");
    }
}
