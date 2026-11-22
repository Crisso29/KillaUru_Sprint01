package pe.killauru.app.sincronizacion.dominio

/**
 * Respuesta 200 de un lote (contrato 005) traducida al dominio: cada evento aparece en una sola lista.
 * La interpreta [InterpretarRespuestaLote] (FR-031).
 */
internal data class RespuestaLote(
    val aceptados: List<String>,
    /** Ya estaban en el servidor, por ejemplo al reintentar tras perder el 200 (EQ-02). */
    val duplicados: List<String>,
    val rechazados: List<Rechazo>,
)
