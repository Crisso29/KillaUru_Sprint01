package pe.killauru.app.deteccion.datos

/** FR-015: normaliza las muestras del acelerómetro según el manifiesto del modelo. */
internal class NormalizadorEntrada(
    private val media: Float,
    private val desviacion: Float,
    private val longitudEsperada: Int,
) {
    fun normalizar(muestras: List<FloatArray>): FloatArray {
        val salida = FloatArray(longitudEsperada)
        val step = if (muestras.isEmpty()) 1 else maxOf(1, muestras.size / (longitudEsperada / 3))
        var idx = 0
        for (i in muestras.indices step step) {
            if (idx + 2 >= longitudEsperada) break
            val m = muestras[i]
            salida[idx++] = (m.getOrElse(0) { 0f } - media) / desviacion
            salida[idx++] = (m.getOrElse(1) { 0f } - media) / desviacion
            salida[idx++] = (m.getOrElse(2) { 0f } - media) / desviacion
        }
        return salida
    }
}
