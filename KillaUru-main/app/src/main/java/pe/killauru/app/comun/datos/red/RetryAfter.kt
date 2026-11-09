package pe.killauru.app.comun.datos.red

import java.time.Duration
import okhttp3.Response

/** FR-036: extrae el Retry-After de una respuesta 429/503. */
internal object RetryAfter {
    fun de(response: Response): Duration? {
        val valor = response.header("Retry-After") ?: return null
        return valor.toLongOrNull()?.let { Duration.ofSeconds(it) }
    }
}
