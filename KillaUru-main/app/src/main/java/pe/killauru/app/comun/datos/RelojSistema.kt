package pe.killauru.app.comun.datos

import android.os.SystemClock
import java.time.Duration

/**
 * FR-026, R-026: único punto del código que lee el reloj del sistema operativo.
 * RelojProhibidoTest (Konsist) verifica que ningún otro archivo acceda a System.currentTimeMillis.
 */
internal object RelojSistema {
    /** Tiempo transcurrido desde el arranque en ms (monotónico, no afectado por NTP). */
    fun elapsedRealtimeMs(): Long = SystemClock.elapsedRealtime()

    /** Tiempo transcurrido desde el arranque como Duration. */
    fun elapsed(): Duration = Duration.ofMillis(elapsedRealtimeMs())

    /** Número de arranques del sistema (para detectar reinicio). */
    fun bootCount(): Int = android.provider.Settings.Global.getInt(null, "boot_count", 0)
}
