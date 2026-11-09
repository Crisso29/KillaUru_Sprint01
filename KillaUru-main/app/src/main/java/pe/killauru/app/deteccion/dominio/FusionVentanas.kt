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
            val clasificacion = par.second
            if (clasificacion.tipo == null) continue
            val p = pendiente
            if (p == null) {
                pendiente = par
            } else {
                pendiente = fusionarPendiente(p, par, resultado)
            }
        }
        pendiente?.let { resultado.add(it) }
        return resultado
    }
    private fun fusionarPendiente(p: Pair<Ventana, Clasificacion>, par: Pair<Ventana, Clasificacion>,
        resultado: MutableList<Pair<Ventana, Clasificacion>>): Pair<Ventana, Clasificacion> {
                val diffMs = ChronoUnit.MILLIS.between(p.first.inicioUtc, par.first.inicioUtc)
                return if (diffMs <= ventanaMsFusion) {
                    if (par.second.confidence >= p.second.confidence) par else p
                } else {
                    resultado.add(p)
                    par
                }
    }
}
