package pe.killauru.app.deteccion.datos

import pe.killauru.app.deteccion.dominio.Ventana
import java.time.Instant

/**
 * FR-011, R-018: acumula muestras del acelerómetro en ventanas de 2 s con overlap 50 %.
 * A 50 Hz: 100 muestras por ventana, nueva ventana cada 50 muestras.
 */
internal class BufferVentanas(
    private val tamano: Int = 100,
    private val overlap: Int = 50,
    private val onVentana: suspend (Ventana) -> Unit,
) {
    private val buffer = ArrayDeque<FloatArray>()
    private var inicioUtc: Instant = Instant.EPOCH

    suspend fun agregar(muestra: FloatArray, utc: Instant) {
        if (buffer.isEmpty()) inicioUtc = utc
        buffer.addLast(muestra)
        if (buffer.size >= tamano) {
            onVentana(Ventana(buffer.toList(), inicioUtc))
            repeat(overlap) { buffer.removeFirst() }
            inicioUtc = utc
        }
    }

    fun limpiar() { buffer.clear() }
}
