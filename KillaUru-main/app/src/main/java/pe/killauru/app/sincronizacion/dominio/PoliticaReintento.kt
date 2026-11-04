package pe.killauru.app.sincronizacion.dominio

import java.time.Duration
import kotlin.math.roundToLong

/** RF-07 / FR-035…FR-036: cálculo puro; WorkManager decide cuándo ejecutar. */
internal class PoliticaReintento(
    private val pasosSegundos: List<Long>,
    jitterPorcentaje: Int,
) {
    private val jitter = jitterPorcentaje / 100.0

    fun espera(paso: Int, uniforme: Double): Duration {
        val base = Duration.ofSeconds(pasosSegundos[paso.coerceIn(pasosSegundos.indices)])
        return escalar(base, 1 + (2 * uniforme - 1) * jitter)
    }

    // Retry-After es un mínimo del servidor: nunca aplicar jitter negativo.
    fun esperaRetryAfter(minimo: Duration, uniforme: Double): Duration =
        escalar(minimo, 1 + uniforme * jitter)

    private fun escalar(base: Duration, factor: Double): Duration =
        Duration.ofMillis((base.toMillis() * factor).roundToLong())
}
