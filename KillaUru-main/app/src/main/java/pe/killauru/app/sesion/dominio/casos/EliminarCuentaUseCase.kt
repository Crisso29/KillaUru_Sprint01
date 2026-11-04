package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import javax.inject.Inject

/** FR-067: elimina la cuenta confirmada con sesión activa. */
internal class EliminarCuentaUseCase @Inject constructor(
    private val api: ApiCuenta,
    private val repoSesion: RepositorioSesion,
    private val limpiezaLocal: LimpiezaLocalSesion,
) {
    suspend fun ejecutar(contrasena: String): ResultadoCuenta {
        val sesion = repoSesion.obtener()
            ?: return ResultadoCuenta.Error(IllegalStateException("Sin sesión activa"))
        val ok = api.eliminarCuenta(sesion.usuarioId, sesion.token)
        return if (ok) {
            limpiezaLocal.ejecutar()
            ResultadoCuenta.Exitoso(sesion)
        } else {
            ResultadoCuenta.Error(RuntimeException("No se pudo eliminar la cuenta"))
        }
    }
}
