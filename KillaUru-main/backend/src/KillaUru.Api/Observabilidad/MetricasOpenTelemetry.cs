using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using KillaUru.Shared.Metricas;

namespace KillaUru.Api.Observabilidad;

/// <summary>
/// Adaptador de <see cref="IMetricas"/> sobre System.Diagnostics.Metrics (data-model §3.2). Las únicas
/// etiquetas son catálogos cerrados (resultado, motivo, tarea, acción): nunca seudónimo, IP ni eventoId (RNF-32).
/// </summary>
internal sealed class MetricasOpenTelemetry : IMetricas
{
    internal const string NombreMedidor = "KillaUru";

    private readonly TimeProvider _reloj;
    private readonly Counter<long> _eventos;
    private readonly Counter<long> _rechazos;
    private readonly Counter<long> _descartes;
    private readonly Counter<long> _retenidos;
    private readonly Histogram<double> _procesoMs;
    private readonly Histogram<double> _retrasoColaS;
    private readonly Counter<long> _accionesRiesgo;
    private readonly ConcurrentDictionary<string, long> _ultimaOk = new(StringComparer.Ordinal);
    private readonly Meter _medidor;

    public MetricasOpenTelemetry(IMeterFactory fabrica, TimeProvider reloj)
    {
        ArgumentNullException.ThrowIfNull(fabrica);
        _reloj = reloj;
        _medidor = fabrica.Create(NombreMedidor);
        _eventos = _medidor.CreateCounter<long>("killauru.ingesta.eventos");
        _rechazos = _medidor.CreateCounter<long>("killauru.ingesta.rechazos");
        _descartes = _medidor.CreateCounter<long>("killauru.anomalias.descartes");
        _retenidos = _medidor.CreateCounter<long>("killauru.anomalias.retenidos_tope");
        _procesoMs = _medidor.CreateHistogram<double>("killauru.anomalias.proceso_ms", "ms");
        _retrasoColaS = _medidor.CreateHistogram<double>("killauru.ingesta.retraso_cola_s", "s");
        _accionesRiesgo = _medidor.CreateCounter<long>("killauru.seguridad.acciones_riesgo");
        _medidor.CreateObservableGauge("killauru.tarea.ultima_ok", () =>
            _ultimaOk.Select(t => new Measurement<long>(t.Value, new KeyValuePair<string, object?>("tarea", t.Key))), "s");
    }

    public void RegistrarLote(int aceptados, int duplicados, int rechazados)
    {
        _eventos.Add(aceptados, new KeyValuePair<string, object?>("resultado", "aceptado"));
        _eventos.Add(duplicados, new KeyValuePair<string, object?>("resultado", "duplicado"));
        _eventos.Add(rechazados, new KeyValuePair<string, object?>("resultado", "rechazado"));
    }

    public void RegistrarRechazo(string motivo) => _rechazos.Add(1, new KeyValuePair<string, object?>("motivo", motivo));

    public void RegistrarDescarte(string motivo) => _descartes.Add(1, new KeyValuePair<string, object?>("motivo", motivo));

    public void RegistrarRetenidoPorTope() => _retenidos.Add(1);

    public void RegistrarDuracionProceso(TimeSpan duracion) => _procesoMs.Record(duracion.TotalMilliseconds);

    public void RegistrarRetrasoCola(TimeSpan retraso) => _retrasoColaS.Record(retraso.TotalSeconds);

    public void ObservarCola(Func<(int Tamano, double AntiguedadSegundos)> lectura)
    {
        ArgumentNullException.ThrowIfNull(lectura);
        _medidor.CreateObservableGauge("killauru.ingesta.cola_tamano", () => lectura().Tamano);
        _medidor.CreateObservableGauge("killauru.ingesta.cola_antiguedad_s", () => lectura().AntiguedadSegundos, "s");
    }

    public void RegistrarTareaCorrecta(string tarea) => _ultimaOk[tarea] = _reloj.GetUtcNow().ToUnixTimeSeconds();

    public void RegistrarAccionRiesgo(string accion) => _accionesRiesgo.Add(1, new KeyValuePair<string, object?>("accion", accion));
}
