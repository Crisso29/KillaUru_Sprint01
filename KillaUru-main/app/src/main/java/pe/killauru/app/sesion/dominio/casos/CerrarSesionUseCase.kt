package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.RepositorioSesion
import javax.inject.Inject

/** FR-066: cierra sesión revocando el token; si sin red, deja revocación pendiente. */
internal class CerrarSesionUseCase @Inject constructor(
    private val api: ApiCuenta,
    private val repoSesion: RepositorioSesion,
    private val limpiezaLocal: LimpiezaLocalSesion,
) {
    suspend fun ejecutar() {
        val sesion = repoSesion.obtener()
        if (sesion != null) {
            try { api.cerrarSesion(sesion.tokenAcceso) } catch (_: Exception) { /* red no disponible */ }
        }
        limpiezaLocal.ejecutar()
    }
}
