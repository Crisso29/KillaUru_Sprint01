package pe.killauru.app.comun.datos

import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.intPreferencesKey
import androidx.datastore.preferences.core.longPreferencesKey
import androidx.datastore.preferences.core.stringPreferencesKey
import java.time.Duration
import java.time.Instant
import kotlinx.coroutines.flow.firstOrNull
import kotlinx.coroutines.flow.map
import pe.killauru.app.comun.dominio.AnclaTiempo

/** R-003: persiste el ancla GPS o del servidor en DataStore; sobrevive a Force Stop. */
internal class AnclaTiempoDataStore(private val store: DataStore<Preferences>) {
    companion object {
        private val K_UTC = longPreferencesKey("ancla.utcMs")
        private val K_ELAPSED = longPreferencesKey("ancla.elapsedMs")
        private val K_BOOT = intPreferencesKey("ancla.bootCount")
        private val K_FUENTE = stringPreferencesKey("ancla.fuente")
    }

    suspend fun guardar(utc: Instant, elapsed: Duration, bootCount: Int, fuente: String) {
        store.edit { p ->
            p[K_UTC] = utc.toEpochMilli()
            p[K_ELAPSED] = elapsed.toMillis()
            p[K_BOOT] = bootCount
            p[K_FUENTE] = fuente
        }
    }

    suspend fun leer(): AnclaTiempo? = store.data.map { p ->
        val utcMs = p[K_UTC] ?: return@map null
        val elapsedMs = p[K_ELAPSED] ?: return@map null
        val boot = p[K_BOOT] ?: return@map null
        AnclaTiempo(Instant.ofEpochMilli(utcMs), Duration.ofMillis(elapsedMs), boot)
    }.firstOrNull()
}
