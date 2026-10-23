using Serilog.Core;
using Serilog.Events;

namespace KillaUru.Api.Observabilidad;

/// <summary>
/// Quita propiedades de primer nivel de Serilog por su nombre (T-SEG03, RN-09, RNF-10).
/// Si existe LoteId en ese nivel, también quita los nombres de sujeto reconocidos.
/// </summary>
/// <remarks>
/// No inspecciona valores anidados, el texto del mensaje ni excepciones, y no procesa trazas de
/// OpenTelemetry. Por sí solo no garantiza FR-034: esos caminos necesitan protección y pruebas propias.
/// </remarks>
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
