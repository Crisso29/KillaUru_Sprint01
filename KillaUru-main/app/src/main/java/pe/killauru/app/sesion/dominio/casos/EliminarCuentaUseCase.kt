package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import javax.inject.Inject

/** FR-067: elimina la cuenta tras confirmar contraseña. */
internal class EliminarCuentaUseCase @Inject constructor(
    private val api: ApiCuenta,
    private val repoSesion: RepositorioSesion,
    private val limpiezaLocal: LimpiezaLocalSesion,
) {
    suspend fun ejecutar(contrasena: String): ResultadoCuenta {
        val sesion = repoSesion.obtener() ?: return ResultadoCuenta.SesionInvalida
        val resultado = api.eliminarCuenta(sesion.tokenAcceso, contrasena)
        if (resultado is ResultadoCuenta.Exito) limpiezaLocal.ejecutar()
        return resultado
    }
}
