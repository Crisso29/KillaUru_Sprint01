package pe.killauru.app.seudonimo.datos

import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import pe.killauru.app.seudonimo.dominio.RepositorioSeudonimo
import javax.inject.Inject
import javax.inject.Singleton

/** FR-039: persiste el seudónimo en PreferenciasDataStore. */
@Singleton
internal class RepositorioSeudonimoDataStore @Inject constructor(
    private val prefs: RepositorioPreferencias,
) : RepositorioSeudonimo {

    override suspend fun obtener(): String? = prefs.obtenerSeudonimo()

    override suspend fun obtenerDesdeUtcMs(): Long? = prefs.obtenerSeudonimoDesdeUtc()

    override suspend fun guardar(seudonimo: String, desdeUtcMs: Long) {
        prefs.guardarSeudonimo(seudonimo, desdeUtcMs)
    }
}
