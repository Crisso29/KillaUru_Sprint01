package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.historial.dominio.RepositorioRecorridos
import pe.killauru.app.mapa.dominio.RecortarAlDemoUseCase
import javax.inject.Inject

/**
 * FR-051, R-023: borra tokens, permiso y historial; recorta anomalías al demo;
 * deja eventos PENDING para envío sin cuenta (con seudonimo).
 */
internal class LimpiezaLocalSesion @Inject constructor(
    private val repoSesion: RepositorioSesion,
    private val repoPrefs: RepositorioPreferencias,
    private val recorridos: RepositorioRecorridos,
    private val recortarAlDemo: RecortarAlDemoUseCase,
) {
    suspend fun ejecutar() {
        repoSesion.cerrar()
        repoPrefs.limpiarSesion()
        recorridos.borrarTodo() // FR-069: Mis viajes solo existe con cuenta
        recortarAlDemo.ejecutar() // FR-051: el anónimo solo ve el tramo demo; lo demás no queda en el celular
    }
}
