package pe.killauru.app.deteccion.dominio

import java.time.Instant
import java.time.temporal.ChronoUnit

/**
 * FR-014, R-018: dos detecciones positivas a ≤ 1 000 ms se fusionan en la de mayor
 * confidence; si están a > 1 000 ms son eventos separados.
 */
internal class FusionVentanas(private val ventanaMsFusion: Long = 1_000L) {

    fun fusionar(clasificaciones: List<Pair<Ventana, Clasificacion>>): List<Pair<Ventana, Clasificacion>> {
        if (clasificaciones.isEmpty()) return emptyList()
        val resultado = mutableListOf<Pair<Ventana, Clasificacion>>()
        var pendiente: Pair<Ventana, Clasificacion>? = null

        for (par in clasificaciones) {
            val (ventana, clasificacion) = par
            if (clasificacion.tipo == null) continue
            val p = pendiente
            if (p == null) {
                pendiente = par
            } else {
                val diffMs = ChronoUnit.MILLIS.between(p.first.inicioUtc, ventana.inicioUtc)
                pendiente = if (diffMs <= ventanaMsFusion) {
                    if (clasificacion.confidence >= p.second.confidence) par else p
                } else {
                    resultado.add(p)
                    par
                }
            }
        }
        pendiente?.let { resultado.add(it) }
        return resultado
    }
}
