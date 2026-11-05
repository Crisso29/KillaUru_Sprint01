package pe.killauru.app.sincronizacion.dominio

/** FR-032, FR-033: divide la lista de IDs en lotes y subdivide culpables. */
internal class PlanParticion(private val maxPorLote: Int = 50) {
    fun partir(ids: List<String>): List<List<String>> = ids.chunked(maxPorLote)

    /** Si el lote dio 400/413 lo subdivide a la mitad; vacío si ya es de 1 (es el culpable). */
    fun partirCulpable(ids: List<String>): List<List<String>> {
        if (ids.size <= 1) return emptyList()
        val mitad = ids.size / 2
        return listOf(ids.take(mitad), ids.drop(mitad))
    }
}
