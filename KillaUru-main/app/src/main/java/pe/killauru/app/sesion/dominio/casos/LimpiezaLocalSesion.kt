package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.historial.dominio.RepositorioRecorridos
import javax.inject.Inject

/**
 * FR-051, R-023: borra tokens, permiso y historial; recorta anomalías al demo;
 * deja eventos PENDING para envío sin cuenta (con seudonimo).
 */
internal class LimpiezaLocalSesion @Inject constructor(
    private val repoSesion: RepositorioSesion,
    private val repoPrefs: RepositorioPreferencias,
    private val recorridos: RepositorioRecorridos,
) {
    suspend fun ejecutar() {
        repoSesion.cerrar()
        repoPrefs.limpiarSesion()
        recorridos.borrarTodo() // FR-069: Mis viajes solo existe con cuenta
    }
}
