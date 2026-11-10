package pe.killauru.app.mapa.dominio

/**
 * data-model §1: estado de la copia local de anomalías.
 * Una sola fila en la BD (id = 1); se guarda en la misma transacción que altas y bajas (R-011).
 */
internal data class EstadoCopia(
    val alcance: Alcance,
    /** Próximo `updatedSince`; null = descarga completa pendiente. */
    val generadoUtc: Long?,
    /** Solo para anónimos: `ETag` de la última respuesta 200. */
    val etag: String?,
    /** Hora del servidor de la última respuesta 200/304 (para «Actualizado hace…»). */
    val verificadoUtc: Long?,
)
