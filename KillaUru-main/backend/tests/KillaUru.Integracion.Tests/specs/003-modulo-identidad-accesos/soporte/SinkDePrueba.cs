using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;
namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>Captura también propiedades estructuradas para detectar secretos fuera del mensaje.</summary>
internal sealed class SinkDePrueba(TextWriter salida) : ILogEventSink
{
    private readonly JsonFormatter formato = new();
    public void Emit(LogEvent logEvent)
    {
        lock (salida) formato.Format(logEvent, salida);
    }
}
