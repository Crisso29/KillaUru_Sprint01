package pe.killauru.app.sincronizacion.datos

import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import javax.inject.Inject
import javax.inject.Singleton

/** FR-035: persiste el paso actual del backoff exponencial. */
@Singleton
internal class EstadoBackoffDataStore @Inject constructor(
    private val prefs: RepositorioPreferencias,
) {
    suspend fun obtenerPaso(): Int = prefs.obtenerBackoffPaso()
    suspend fun guardarPaso(paso: Int) { prefs.guardarBackoffPaso(paso) }
    suspend fun resetear() { prefs.guardarBackoffPaso(0) }
}
