package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import pe.killauru.app.sesion.dominio.Rol
import javax.inject.Inject

/** FR-061, FR-062, R-033: inicia sesión y guarda solo sesiones de conductor; una de administración se rechaza. */
internal class IniciarSesionUseCase @Inject constructor(
    private val api: ApiCuenta,
    private val repoSesion: RepositorioSesion,
) {
    suspend fun ejecutar(correo: String, contrasena: String): ResultadoCuenta {
        val resultado = api.iniciarSesion(correo, contrasena)
        if (resultado !is ResultadoCuenta.Exitoso) return resultado
        if (resultado.sesion.rol != Rol.ConductorRegistrado) return ResultadoCuenta.CuentaDeAdministracion
        repoSesion.guardar(resultado.sesion)
        return resultado
    }
}
