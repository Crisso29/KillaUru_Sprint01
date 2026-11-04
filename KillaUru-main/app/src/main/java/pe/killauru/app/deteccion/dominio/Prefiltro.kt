package pe.killauru.app.deteccion.dominio

/** FR-011, R-018: descarta ventanas con varianza de la norma por debajo del umbral. */
internal class Prefiltro(private val varianzaMinima: Double) {

    fun pasar(ventana: Ventana): Boolean {
        if (ventana.muestras.isEmpty()) return false
        val normas = ventana.muestras.map { m ->
            val ax = m.getOrElse(0) { 0f }
            val ay = m.getOrElse(1) { 0f }
            val az = m.getOrElse(2) { 0f }
            Math.sqrt((ax * ax + ay * ay + az * az).toDouble())
        }
        val media = normas.average()
        val varianza = normas.map { (it - media) * (it - media) }.average()
        return varianza >= varianzaMinima
    }
}
