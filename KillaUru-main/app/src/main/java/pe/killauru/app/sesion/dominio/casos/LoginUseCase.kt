package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import javax.inject.Inject

/** FR-063: inicia sesión con correo y contraseña. */
internal class LoginUseCase @Inject constructor(
    private val api: ApiCuenta,
    private val repo: RepositorioSesion,
) {
    suspend fun ejecutar(correo: String, contrasena: String): ResultadoCuenta {
        return when (val resultado = api.login(correo, contrasena)) {
            is ResultadoCuenta.Exitoso -> {
                repo.guardar(resultado.sesion)
                resultado
            }
            else -> resultado
        }
    }
}
