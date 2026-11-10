package pe.killauru.app.mapa.dominio

/**
 * FR-040, FR-049, FR-052: contrato del repositorio de anomalías locales.
 * Altas, bajas y `EstadoCopia` deben guardarse en la misma transacción (R-011).
 */
internal interface RepositorioAnomalias {
    /** Devuelve las anomalías cuyo `venceUtc > ahoraMs`. */
    suspend fun vigentes(ahoraMs: Long): List<AnomaliaLocal>

    /**
     * Aplica un lote de altas/bajas y persiste el nuevo [estado] en la misma transacción.
     * El [estado] contiene el `generadoUtc` del servidor para el próximo `updatedSince`.
     */
    suspend fun actualizarLote(
        altas: List<AnomaliaLocal>,
        bajas: List<String>,
        estado: EstadoCopia,
    )

    /** Mantiene solo las anomalías con `enDemo = true`. */
    suspend fun recortarAlDemo()

    /** Devuelve el estado de la última descarga; null si nunca se descargó. */
    suspend fun obtenerEstado(): EstadoCopia?

    /** Borra todas las anomalías y el estado de la copia (al iniciar sesión con otro rol). */
    suspend fun limpiarCopia()
}
