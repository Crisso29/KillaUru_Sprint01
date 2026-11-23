using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;
namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>
/// Captura los logs como JSON, con sus propiedades estructuradas, para detectar secretos fuera del mensaje.
/// Seguro entre hilos (DT-04): los servicios en segundo plano del host escriben mientras la prueba lee. El texto es
/// privado y solo se lee con <see cref="Capturado"/>, bajo el mismo candado que la escritura; antes la prueba leía el
/// <c>StringWriter</c> sin candado y fallaba de vez en cuando con <c>ArgumentOutOfRangeException … chunkLength</c>.
/// </summary>
internal sealed class SinkDePrueba : ILogEventSink
{
    private readonly JsonFormatter formato = new();
    private readonly StringBuilder texto = new();
    private readonly Lock candado = new();

    /// <summary>Copia de todo lo registrado hasta ahora.</summary>
    public string Capturado
    {
        get
        {
            lock (candado) return texto.ToString();
        }
    }

    public void Emit(LogEvent logEvent)
    {
        lock (candado)
        {
            // Cerrar el escritor no toca el StringBuilder: solo es el adaptador que pide el formateador
            using var escritor = new StringWriter(texto, System.Globalization.CultureInfo.InvariantCulture);
            formato.Format(logEvent, escritor);
        }
    }
}
