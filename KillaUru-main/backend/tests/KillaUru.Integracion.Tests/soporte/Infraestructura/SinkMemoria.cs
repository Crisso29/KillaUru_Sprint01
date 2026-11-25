using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Collections.Concurrent;

namespace KillaUru.Integracion.Tests.Infraestructura;

/// <summary>
/// Sink de Serilog para pruebas: captura los eventos ya enriquecidos (después de FiltroDatosPersonales)
/// como JSON renderizado. Permite verificar que las propiedades sensibles fueron eliminadas (T067).
/// </summary>
internal sealed class SinkMemoria : ILogEventSink
{
    private static readonly RenderedCompactJsonFormatter Formato = new();
    private readonly ConcurrentQueue<string> _lineas = new();

    internal IReadOnlyCollection<string> Lineas => _lineas;

    internal bool ContienePropiedadConValor(string nombre, string valor)
    {
        var fragmento = $"\"{nombre}\":\"{valor}\"";
        return _lineas.Any(l => l.Contains(fragmento, StringComparison.OrdinalIgnoreCase));
    }

    internal bool ContieneTexto(string texto) =>
        _lineas.Any(l => l.Contains(texto, StringComparison.OrdinalIgnoreCase));

    public void Emit(LogEvent logEvent)
    {
        using var w = new StringWriter();
        Formato.Format(logEvent, w);
        _lineas.Enqueue(w.ToString());
    }
}
