package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import pe.killauru.app.sesion.dominio.ValidadorContrasena
import javax.inject.Inject

/** FR-065: registra una nueva cuenta con correo y contraseña válidos. */
internal class RegistrarseUseCase @Inject constructor(
    private val api: ApiCuenta,
    private val repo: RepositorioSesion,
) {
    suspend fun ejecutar(correo: String, contrasena: String): ResultadoCuenta {
        if (!ValidadorContrasena.esValida(contrasena)) {
            return ResultadoCuenta.CredencialesInvalidas
        }
        return when (val resultado = api.registrar(correo, contrasena)) {
            is ResultadoCuenta.Exitoso -> {
                repo.guardar(resultado.sesion)
                resultado
            }
            else -> resultado
        }
    }
}
