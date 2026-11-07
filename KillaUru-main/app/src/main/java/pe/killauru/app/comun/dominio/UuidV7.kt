package pe.killauru.app.comun.dominio

import java.security.SecureRandom
import java.time.Instant
import java.util.UUID

/** FR-013 / R-004: 48 bits de hora GPS y 74 aleatorios, sin consultar el reloj global. */
internal object UuidV7 {
    fun de(detectadoUtc: Instant, aleatorio: SecureRandom): UUID {
        val milisegundos = detectadoUtc.toEpochMilli()
        val superiores = (milisegundos shl BITS_BAJOS_HORA) or VERSION or
            (aleatorio.nextLong() and MASCARA_ALEATORIA_SUPERIOR)
        val inferiores = (aleatorio.nextLong() and MASCARA_ALEATORIA_INFERIOR) or Long.MIN_VALUE
        return UUID(superiores, inferiores)
    }

    // Constantes del formato binario RFC 9562, no umbrales del negocio.
    private const val BITS_BAJOS_HORA = 16
    private const val VERSION = 0x7000L
    private const val MASCARA_ALEATORIA_SUPERIOR = 0xFFFL
    private const val MASCARA_ALEATORIA_INFERIOR = 0x3FFFFFFFFFFFFFFFL
}
