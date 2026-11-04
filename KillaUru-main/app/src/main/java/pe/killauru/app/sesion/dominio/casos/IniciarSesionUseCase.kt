package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import pe.killauru.app.sesion.dominio.RepositorioSesion
import javax.inject.Inject

/** FR-061: inicia sesión; R-033: rechaza rol Administrador. */
internal class IniciarSesionUseCase @Inject constructor(
    private val api: ApiCuenta,
    private val repoSesion: RepositorioSesion,
) {
    suspend fun ejecutar(correo: String, contrasena: String): ResultadoCuenta {
        val result = api.iniciarSesion(correo, contrasena)
        return if (result.isSuccess) {
            val sesion = result.getOrThrow()
            if (sesion.rol == "Administrador") return ResultadoCuenta.RolNoPermitido
            repoSesion.guardar(sesion)
            ResultadoCuenta.Exito
        } else {
            ResultadoCuenta.Error("Correo o contraseña incorrectos")
        }
    }
}
