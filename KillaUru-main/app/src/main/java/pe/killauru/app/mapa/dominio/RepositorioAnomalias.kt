package pe.killauru.app.mapa.dominio

/** FR-040, FR-049: contrato del repositorio de anomalías locales. */
internal interface RepositorioAnomalias {
    suspend fun vigentes(ahoraMs: Long): List<AnomaliaLocal>
    suspend fun actualizarLote(altas: List<AnomaliaLocal>, bajas: List<String>)
    suspend fun recortarAlDemo()
}
