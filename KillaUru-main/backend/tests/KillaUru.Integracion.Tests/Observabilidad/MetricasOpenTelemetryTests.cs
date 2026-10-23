using System.Diagnostics.Metrics;
using KillaUru.Api.Observabilidad;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace KillaUru.Integracion.Tests.Observabilidad;

/// <summary>Parte de T049: verifica resultados de lote, unidades del retraso y marca temporal de tareas.</summary>
/// <remarks>Los demás métodos de IMetricas y la restricción de catálogos todavía no tienen casos aquí.</remarks>
public sealed class MetricasOpenTelemetryTests : IDisposable
{
    private readonly ServiceProvider _proveedor = new ServiceCollection().AddMetrics().BuildServiceProvider();
    private readonly FakeTimeProvider _reloj = new(DateTimeOffset.FromUnixTimeSeconds(1_800_000_000));
    private readonly List<(string Instrumento, double Valor, string Etiquetas)> _medidas = [];
    private readonly MeterListener _oyente = new();

    public MetricasOpenTelemetryTests()
    {
        _oyente.InstrumentPublished = (instrumento, oyente) =>
        {
            if (instrumento.Meter.Name == MetricasOpenTelemetry.NombreMedidor)
            {
                oyente.EnableMeasurementEvents(instrumento);
            }
        };
        _oyente.SetMeasurementEventCallback<long>((i, v, t, _) => Anotar(i, v, t));
        _oyente.SetMeasurementEventCallback<double>((i, v, t, _) => Anotar(i, v, t));
        _oyente.Start();
    }

    private MetricasOpenTelemetry Metricas() => new(_proveedor.GetRequiredService<IMeterFactory>(), _reloj);

    [Fact]
    public void RegistrarLote_cuenta_cada_resultado()
    {
        Metricas().RegistrarLote(aceptados: 49, duplicados: 0, rechazados: 1);

        Assert.Contains(("killauru.ingesta.eventos", 49d, "resultado=aceptado"), _medidas);
        Assert.Contains(("killauru.ingesta.eventos", 1d, "resultado=rechazado"), _medidas);
    }

    [Fact]
    public void RegistrarRetrasoCola_mide_en_segundos()
    {
        Metricas().RegistrarRetrasoCola(TimeSpan.FromMilliseconds(1500));

        Assert.Contains(("killauru.ingesta.retraso_cola_s", 1.5, ""), _medidas);
    }

    [Fact]
    public void RegistrarTareaCorrecta_publica_la_hora_unix_de_la_tarea()
    {
        Metricas().RegistrarTareaCorrecta("caducidad");

        _oyente.RecordObservableInstruments();

        Assert.Contains(("killauru.tarea.ultima_ok", 1_800_000_000d, "tarea=caducidad"), _medidas);
    }

    public void Dispose()
    {
        _oyente.Dispose();
        _proveedor.Dispose();
    }

    private void Anotar<T>(Instrument instrumento, T valor, ReadOnlySpan<KeyValuePair<string, object?>> etiquetas)
        where T : struct
    {
        var texto = string.Join(",", etiquetas.ToArray().Select(e => $"{e.Key}={e.Value}"));
        _medidas.Add((instrumento.Name, Convert.ToDouble(valor, System.Globalization.CultureInfo.InvariantCulture), texto));
    }
}
