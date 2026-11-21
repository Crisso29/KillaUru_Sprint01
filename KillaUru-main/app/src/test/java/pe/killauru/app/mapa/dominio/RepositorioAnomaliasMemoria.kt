package pe.killauru.app.mapa.dominio

/** Doble local para aislar la sincronización del mapa. */
internal class RepositorioAnomaliasMemoria : RepositorioAnomalias {
    val anomalias = mutableListOf<AnomaliaLocal>()
    var estado: EstadoCopia? = null
    var limpiezas = 0

    override suspend fun vigentes(ahoraMs: Long) = anomalias.toList()

    override suspend fun actualizarLote(
        altas: List<AnomaliaLocal>,
        bajas: List<String>,
        estado: EstadoCopia,
    ) {
        anomalias.removeAll { it.id in bajas }
        anomalias.addAll(altas)
        this.estado = estado
    }

    override suspend fun recortarAlDemo() {
        anomalias.removeAll { !it.enDemo }
    }

    override suspend fun obtenerEstado() = estado
    override suspend fun limpiarCopia() {
        anomalias.clear()
        estado = null
        limpiezas++
    }
}
