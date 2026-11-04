package pe.killauru.app.actualizador.dominio

/** FR-070, FR-071, R-017: si el modelo es compatible con la app instalada. */
internal class PoliticaActivacionModelo(
    private val versionApp: String,
    private val esquemaCompatible: Int = 1,
    private val etiquetasEsperadasPorDefecto: List<String> = listOf("Normal", "Bache", "Hundimiento"),
) {
    fun puedeActivar(
        manifiesto: ManifiestoModelo,
        etiquetasEsperadas: List<String> = etiquetasEsperadasPorDefecto,
    ): Boolean {
        if (manifiesto.esquema != esquemaCompatible) return false
        if (!esVersionCompatible(manifiesto.versionMinimaApp, versionApp)) return false
        if (manifiesto.etiquetas != etiquetasEsperadas) return false
        return true
    }

    private fun esVersionCompatible(minima: String, actual: String): Boolean {
        val m = minima.split(".").map { it.toIntOrNull() ?: 0 }
        val a = actual.split(".").map { it.toIntOrNull() ?: 0 }
        for (i in 0 until maxOf(m.size, a.size)) {
            val mv = m.getOrElse(i) { 0 }; val av = a.getOrElse(i) { 0 }
            if (av > mv) return true
            if (av < mv) return false
        }
        return true
    }
}
