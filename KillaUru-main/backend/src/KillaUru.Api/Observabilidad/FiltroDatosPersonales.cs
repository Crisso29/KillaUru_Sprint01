using Serilog.Core;
using Serilog.Events;

namespace KillaUru.Api.Observabilidad;

/// <summary>
/// Quita de cada log las propiedades con datos personales o secretos antes de que salgan hacia
/// Grafana (T-SEG03, RN-09, RNF-10): seudónimo, IP, tokens, cookies, correos. Y nunca deja juntos el
/// sujeto del token y un loteId, porque juntos unirían una cuenta con sus eventos.
/// </summary>
internal sealed class FiltroDatosPersonales : ILogEventEnricher
{
    private static readonly string[] Prohibidas =
    [
        "seudonimo", "ip", "remoteip", "clientip", "authorization", "cookie", "token", "correo", "email", "password", "contrasena",
    ];

    private static readonly string[] Sujeto = ["sub", "sujeto", "usuarioid", "cuentaid"];

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        var nombres = logEvent.Properties.Keys.ToList();
        foreach (var nombre in nombres.Where(EsProhibida))
        {
            logEvent.RemovePropertyIfPresent(nombre);
        }

        if (nombres.Any(n => n.Equals("LoteId", StringComparison.OrdinalIgnoreCase)))
        {
            foreach (var nombre in nombres.Where(n => Sujeto.Contains(n.ToLowerInvariant())))
            {
                logEvent.RemovePropertyIfPresent(nombre);
            }
        }
    }

    private static bool EsProhibida(string nombre)
    {
        var minuscula = nombre.ToLowerInvariant();
        return Prohibidas.Any(p => minuscula == p || minuscula.Contains(p, StringComparison.Ordinal) && p.Length > 2);
    }
}
