package pe.killauru.app.sincronizacion.dominio

/** FR-038: puerto para purgar eventos en estados finales. */
internal interface PurgadorEventos {
    suspend fun purgarEventosFinales(anteriorAUtcMs: Long)
}
