package pe.killauru.app.deteccion.datos

import java.time.Instant
import pe.killauru.app.deteccion.dominio.Ventana

/**
 * FR-011, R-018: acumula muestras en un contexto de 2 s (100 muestras a 50 Hz) y emite ventanas
 * de 500 ms (25 muestras) con stride de 13 (~260 ms de overlap).
 * Llama a [DescomposicionEjes] para separar componente vertical, horizontal y giro.
 */
internal class BufferVentanas(
    private val tamanoVentana: Int = 25,
    private val tamanoContexto: Int = 100,
    private val stride: Int = 13,
    private val descomposicion: DescomposicionEjes,
    private val velocidadKmH: () -> Float,
    private val onVentana: suspend (Ventana) -> Unit,
) {
    private val accelBuf = ArrayDeque<FloatArray>()
    private val gyroBuf = ArrayDeque<FloatArray>()
    private var contadorStride = 0
    private var proximoInicio = Instant.EPOCH

    suspend fun agregar(accel: FloatArray, gyro: FloatArray, utc: Instant) {
        if (accelBuf.isEmpty()) proximoInicio = utc
        accelBuf.addLast(accel.copyOf())
        gyroBuf.addLast(gyro.copyOf())
        while (accelBuf.size > tamanoContexto) { accelBuf.removeFirst(); gyroBuf.removeFirst() }
        contadorStride++
        if (accelBuf.size >= tamanoContexto && contadorStride >= stride) {
            val ctx = accelBuf.toTypedArray()
            val n = tamanoVentana
            val g25 = Array(n) { i -> gyroBuf[gyroBuf.size - n + i] }
            onVentana(descomposicion.descomponer(ctx, g25, velocidadKmH(), proximoInicio))
            proximoInicio = utc
            contadorStride = 0
        }
    }

    fun limpiar() { accelBuf.clear(); gyroBuf.clear(); contadorStride = 0 }
}
