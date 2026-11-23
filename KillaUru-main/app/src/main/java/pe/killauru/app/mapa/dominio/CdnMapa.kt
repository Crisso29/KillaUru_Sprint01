package pe.killauru.app.mapa.dominio

/** Respuesta al pedir el manifiesto: uno nuevo (con su ETag), sin cambios (304) o no se pudo (red, formato). */
internal sealed interface RespuestaManifiesto {
    data class Nuevo(val manifiesto: ManifiestoMapa, val etag: String?) : RespuestaManifiesto
    data object SinCambios : RespuestaManifiesto
    data class Fallo(val causa: Throwable) : RespuestaManifiesto
}

/** FR-042: puerto hacia la CDN pública del mapa. Con [etagPrevio] la CDN responde 304 si nada cambió. */
internal interface CdnMapa {
    suspend fun manifiesto(etagPrevio: String?): RespuestaManifiesto
}
