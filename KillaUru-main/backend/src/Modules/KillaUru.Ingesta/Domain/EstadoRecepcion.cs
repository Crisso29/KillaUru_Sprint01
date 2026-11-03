namespace KillaUru.Ingesta.Domain;

/// <summary>Ciclo de vida de una fila en evento_recibido (data-model §2).</summary>
internal enum EstadoRecepcion : short
{
    /// <summary>Guardado tras el INSERT; aún no entregado a Anomalías.</summary>
    Recibido = 0,

    /// <summary>Entregado a Anomalías con resultado Procesado o YaProcesado.</summary>
    Procesado = 1,

    /// <summary>Entregado a Anomalías y descartado por sus reglas.</summary>
    Descartado = 2,
}
