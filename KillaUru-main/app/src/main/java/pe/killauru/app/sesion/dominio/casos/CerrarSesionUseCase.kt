package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import javax.inject.Inject

/** FR-069: cierra sesión limpiando token y preferencias de identidad. */
internal class CerrarSesionUseCase @Inject constructor(
    private val repo: RepositorioSesion,
    private val prefs: RepositorioPreferencias,
) {
    suspend fun ejecutar() {
        repo.cerrar()
        prefs.limpiarSesion()
    }
}
