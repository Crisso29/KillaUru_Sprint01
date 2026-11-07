package pe.killauru.app.deteccion.dominio

import java.time.Duration
import java.time.Instant

/** FR-018, R-019: inicia un viaje con RN-03 y lo termina tras 30 min detenido. */
internal class DetectorViaje(
    private val tiempoParadoMaximo: Duration = Duration.ofMinutes(30),
    private val generarId: () -> String,
) {
    private var viajeActual: Viaje? = null
    private var ultimoMovimientoUtc: Instant? = null

    val viaje: Viaje? get() = viajeActual

    fun registrar(enMovimiento: Boolean, ahora: Instant, kmActual: Double): Viaje? {
        return if (enMovimiento) {
            ultimoMovimientoUtc = ahora
            if (viajeActual == null) {
                viajeActual = Viaje(generarId(), ahora, kmActual)
            }
            viajeActual
        } else {
            val ultimo = ultimoMovimientoUtc
            if (ultimo != null && Duration.between(ultimo, ahora) >= tiempoParadoMaximo) {
                terminar()
            }
            viajeActual
        }
    }

    fun terminar(): Viaje? {
        val v = viajeActual
        viajeActual = null
        ultimoMovimientoUtc = null
        return v
    }
}
