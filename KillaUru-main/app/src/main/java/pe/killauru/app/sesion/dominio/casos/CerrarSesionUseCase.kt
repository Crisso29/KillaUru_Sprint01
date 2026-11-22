package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.RevocacionPendiente
import javax.inject.Inject

/**
 * FR-066, FR-069, RN-21 d, R-023: cerrar sesión. Primero limpia el celular (tokens, permiso e historial), sin esperar a
 * la red; después revoca la sesión en el servidor. Si no hay red, la revocación queda programada. Las detecciones por
 * enviar no se tocan: salen sin cuenta.
 */
internal class CerrarSesionUseCase @Inject constructor(
    private val api: ApiCuenta,
    private val repo: RepositorioSesion,
    private val limpiezaLocal: LimpiezaLocalSesion,
    private val pendiente: RevocacionPendiente,
) {
    suspend fun ejecutar() {
        val sesion = repo.obtener()
        limpiezaLocal.ejecutar()
        if (sesion != null && !api.revocarSesion(sesion.tokenAcceso, sesion.tokenRenovacion)) {
            pendiente.programar(sesion.tokenRenovacion)
        }
    }
}
