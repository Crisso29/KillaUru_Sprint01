package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.ResultadoEliminacion
import javax.inject.Inject

/** FR-067, HU-23: elimina la cuenta confirmando la contraseña; solo un 204 hace la limpieza local. */
internal class EliminarCuentaUseCase @Inject constructor(
    private val api: ApiCuenta,
    private val repoSesion: RepositorioSesion,
    private val limpiezaLocal: LimpiezaLocalSesion,
) {
    suspend fun ejecutar(contrasena: String): ResultadoEliminacion {
        repoSesion.obtener() ?: return ResultadoEliminacion.Error(IllegalStateException("Sin sesión activa"))
        val resultado = api.eliminarCuenta(contrasena)
        if (resultado is ResultadoEliminacion.Eliminada) limpiezaLocal.ejecutar()
        return resultado
    }
}
