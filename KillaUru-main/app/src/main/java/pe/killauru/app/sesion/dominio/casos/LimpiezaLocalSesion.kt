package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.datos.RepositorioPreferenciasDataStore
import javax.inject.Inject

/**
 * FR-051, R-023: borra tokens, permiso y historial; recorta anomalías al demo;
 * deja eventos PENDING para envío sin cuenta (con seudonimo).
 */
internal class LimpiezaLocalSesion @Inject constructor(
    private val repoSesion: RepositorioSesion,
    private val repoPrefs: RepositorioPreferenciasDataStore,
) {
    suspend fun ejecutar() {
        repoSesion.cerrar()
        repoPrefs.limpiarSesion()
    }
}
