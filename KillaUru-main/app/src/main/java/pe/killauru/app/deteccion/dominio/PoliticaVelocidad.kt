package pe.killauru.app.deteccion.dominio

import java.time.Duration
import java.time.Instant

/** RN-03 / FR-010: una caída de velocidad cancela toda la ventana sostenida. */
internal class PoliticaVelocidad(
    private val minimaKmh: Double,
    private val duracionSostenida: Duration,
) {
    private var inicio: Instant? = null

    fun registrar(kmh: Double?, instante: Instant): Boolean {
        if (kmh == null || !kmh.isFinite() || kmh <= minimaKmh) {
            inicio = null
            return false
        }
        val desde = inicio
        if (desde == null || instante < desde) inicio = instante
        return Duration.between(inicio, instante) >= duracionSostenida
    }
}
