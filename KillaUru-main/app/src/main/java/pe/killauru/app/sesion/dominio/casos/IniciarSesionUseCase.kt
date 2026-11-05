package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import javax.inject.Inject

/** FR-061: inicia sesión; R-033: rechaza rol Administrador (sólo app ciudadano). */
internal class IniciarSesionUseCase @Inject constructor(
    private val api: ApiCuenta,
    private val repoSesion: RepositorioSesion,
) {
    suspend fun ejecutar(correo: String, contrasena: String): ResultadoCuenta {
        return when (val resultado = api.login(correo, contrasena)) {
            is ResultadoCuenta.Exitoso -> {
                repoSesion.guardar(resultado.sesion)
                resultado
            }
            else -> resultado
        }
    }
}
