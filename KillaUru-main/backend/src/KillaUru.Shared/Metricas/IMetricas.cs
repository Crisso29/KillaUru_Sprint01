namespace KillaUru.Shared.Metricas;

/// <summary>
/// Puerto de métricas técnicas (RNF-22). Lo implementa el host con OpenTelemetry; los módulos solo
/// lo usan. Nunca recibe datos de alta cardinalidad (seudónimo, IP, eventoId): RNF-32.
/// </summary>
public interface IMetricas
{
    /// <summary>Resultado de un lote recibido (Ingesta).</summary>
    /// <param name="aceptados">Eventos guardados para procesar.</param>
    /// <param name="duplicados">Eventos que ya existían.</param>
    /// <param name="rechazados">Eventos que no cumplieron las reglas de recepción.</param>
    void RegistrarLote(int aceptados, int duplicados, int rechazados);

    /// <summary>Un evento rechazado en la recepción (RN-11).</summary>
    /// <param name="motivo">Motivo del catálogo cerrado <c>MotivoDescarte</c>.</param>
    void RegistrarRechazo(string motivo);

    /// <summary>Un evento descartado al procesar (Anomalías).</summary>
    /// <param name="motivo">Motivo del catálogo cerrado <c>MotivoDescarte</c>.</param>
    void RegistrarDescarte(string motivo);

    /// <summary>Una anomalía nueva retenida por el tope por hora del dispositivo (RN-15a).</summary>
    void RegistrarRetenidoPorTope();

    /// <summary>Duración del procesamiento de un evento (RNF-23).</summary>
    /// <param name="duracion">Tiempo que tomó.</param>
    void RegistrarDuracionProceso(TimeSpan duracion);

    /// <summary>Tiempo entre la recepción y el procesamiento de un evento (RNF-23, D11).</summary>
    /// <param name="retraso">Procesado menos recibido.</param>
    void RegistrarRetrasoCola(TimeSpan retraso);

    /// <summary>Observa el tamaño de la cola y la antigüedad del pendiente más viejo (RNF-22).</summary>
    /// <param name="lectura">Función que se consulta en cada lectura de la métrica.</param>
    void ObservarCola(Func<(int Tamano, double AntiguedadSegundos)> lectura);

    /// <summary>Última ejecución correcta de una tarea programada (alerta a las 26 h, RNF-22).</summary>
    /// <param name="tarea">Nombre del catálogo de tareas (p. ej. <c>caducidad</c>).</param>
    void RegistrarTareaCorrecta(string tarea);

    /// <summary>Una acción de riesgo confirmada (RNF-29). Solo la emite Auditoría, después del commit.</summary>
    /// <param name="accion">Acción del catálogo de auditoría.</param>
    void RegistrarAccionRiesgo(string accion);
}
