package pe.killauru.app.mapa.dominio

/**
 * FR-044, FR-045: puerto de red para obtener el delta de anomalías del servidor.
 * Separa la lógica HTTP de los casos de uso del dominio (R-002).
 */
internal interface FuenteAnomalias {
    sealed class Respuesta {
        /** El servidor devolvió datos (200). */
        data class Datos(
            val altas: List<AnomaliaLocal>,
            val bajas: List<String>,
            val generadoUtc: Long,
            /** Solo en respuestas sin token (anónimas sin filtros). */
            val etag: String?,
        ) : Respuesta()

        /** El servidor respondió 304: la copia sigue vigente. */
        data class SinCambios(val verificadoUtc: Long) : Respuesta()

        /** El servidor respondió 400: descarta los parámetros y descarga completa. */
        object SolicitudInvalida : Respuesta()

        /** Error de red o respuesta inesperada. */
        data class Error(val causa: Throwable) : Respuesta()
    }

    /**
     * Pide el mapa al servidor.
     * @param alcance   rol del usuario
     * @param updatedSince  hora del último `generadoUtc`; null = descarga completa
     * @param etag      valor del `ETag` previo (solo anónimos); null = sin revalidación
     */
    suspend fun obtener(
        alcance: Alcance,
        updatedSince: Long?,
        etag: String?,
    ): Respuesta
}
