package pe.killauru.app.comun.datos

import android.os.SystemClock
import java.time.Duration

/**
 * FR-026, R-026: único punto del código que lee el reloj del sistema operativo.
 * RelojProhibidoTest (Konsist) verifica que ningún otro archivo acceda a System.currentTimeMillis.
 */
internal object RelojSistema {
    fun utcMs(): Long = System.currentTimeMillis()
    /** Tiempo transcurrido desde el arranque en ms (monotónico, no afectado por NTP). */
    fun elapsedRealtimeMs(): Long = SystemClock.elapsedRealtime()

    /** Tiempo transcurrido desde el arranque como Duration. */
    fun elapsed(): Duration = Duration.ofMillis(elapsedRealtimeMs())

    /** Número de arranques del sistema (para detectar reinicio). */
    fun bootCount(resolver: android.content.ContentResolver): Int =
        android.provider.Settings.Global.getInt(resolver, "boot_count", 0)
}
