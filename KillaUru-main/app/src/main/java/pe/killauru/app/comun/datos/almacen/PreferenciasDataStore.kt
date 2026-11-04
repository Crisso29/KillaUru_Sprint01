package pe.killauru.app.comun.datos.almacen

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.booleanPreferencesKey
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.longPreferencesKey
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map

/** data-model §6: DataStore de preferencias no sensibles. */
internal class PreferenciasDataStore(context: Context) {
    companion object {
        val K_CATEGORIA = stringPreferencesKey("categoria")
        val K_SOLO_MAPA = booleanPreferencesKey("soloVerMapa")
        val K_DATOS_MOVILES = booleanPreferencesKey("usarDatosMoviles")
        val K_TERMINOS_VERSION = stringPreferencesKey("terminos.version")
        val K_TERMINOS_ACEPTADO = longPreferencesKey("terminos.aceptadoUtc")
        val K_POLITICA_VERSION = stringPreferencesKey("politica.version")
        val K_POLITICA_ACEPTADO = longPreferencesKey("politica.aceptadoUtc")
        val K_SEUDONIMO = stringPreferencesKey("seudonimo")
        val K_SEUDONIMO_DESDE = longPreferencesKey("seudonimoDesdeUtc")
        val K_BACKOFF_PASO = androidx.datastore.preferences.core.intPreferencesKey("backoff.paso")
        val K_MAPA_VERSION = stringPreferencesKey("mapa.version")
        val K_MODELO_VERSION = stringPreferencesKey("modelo.version")
        val K_AVISO_DEMO_VIAJE = stringPreferencesKey("aviso.demo.viajeId")
    }

    private val Context.dataStore: DataStore<Preferences> by preferencesDataStore("preferencias")
    private val store = context.dataStore

    fun <T> leer(clave: Preferences.Key<T>): Flow<T?> = store.data.map { it[clave] }

    suspend fun <T> guardar(clave: Preferences.Key<T>, valor: T) {
        store.edit { it[clave] = valor }
    }

    suspend fun <T> borrar(clave: Preferences.Key<T>) {
        store.edit { it.remove(clave) }
    }

    suspend fun limpiar() { store.edit { it.clear() } }
}
