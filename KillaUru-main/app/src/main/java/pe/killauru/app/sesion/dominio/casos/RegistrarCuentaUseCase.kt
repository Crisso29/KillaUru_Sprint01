package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import pe.killauru.app.sesion.dominio.SolicitudRegistro
import pe.killauru.app.sesion.dominio.ValidadorContrasena
import javax.inject.Inject

/**
 * FR-060, HU-22, HU-27, D13: registra al conductor con las versiones legales que aceptó y guarda la sesión del 201.
 * Lo que la app puede comprobar sola (contraseña y consentimiento) se valida antes de gastar una petición.
 */
internal class RegistrarCuentaUseCase @Inject constructor(
    private val api: ApiCuenta,
    private val repoSesion: RepositorioSesion,
    private val preferencias: RepositorioPreferencias,
) {
    suspend fun ejecutar(correo: String, contrasena: String): ResultadoCuenta {
        if (!ValidadorContrasena.esValida(contrasena)) return ResultadoCuenta.ContrasenaInvalida
        val politica = preferencias.obtenerPoliticaVersion() ?: return ResultadoCuenta.SinConsentimiento
        val terminos = preferencias.obtenerTerminosVersion() ?: return ResultadoCuenta.SinConsentimiento
        val resultado = api.registrar(SolicitudRegistro(correo, contrasena, politica, terminos))
        if (resultado is ResultadoCuenta.Exitoso) repoSesion.guardar(resultado.sesion)
        return resultado
    }
}
