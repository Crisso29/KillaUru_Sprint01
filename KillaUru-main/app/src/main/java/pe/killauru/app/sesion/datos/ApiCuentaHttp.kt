package pe.killauru.app.sesion.datos

import kotlinx.coroutines.CancellationException
import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import pe.killauru.app.sesion.dominio.ResultadoEliminacion
import pe.killauru.app.sesion.dominio.SolicitudRegistro
import retrofit2.Response
import javax.inject.Inject
import javax.inject.Singleton

/** FR-060…FR-067: implementación HTTP de [ApiCuenta] con el contrato 003; traduce cada código a un resultado de dominio. */
@Singleton
internal class ApiCuentaHttp @Inject constructor(
    private val api: IdentidadApi,
) : ApiCuenta {

    override suspend fun registrar(solicitud: SolicitudRegistro): ResultadoCuenta =
        protegido({ ResultadoCuenta.Error(it) }) { sesion(api.registrar(MapeadorSesion.aDto(solicitud))) }

    override suspend fun iniciarSesion(correo: String, contrasena: String): ResultadoCuenta =
        protegido({ ResultadoCuenta.Error(it) }) { sesion(api.iniciarSesion(InicioSesionSolicitudDto(correo, contrasena, CANAL_APP))) }

    override suspend fun reenviarVerificacion(): Boolean =
        protegido({ false }) { api.reenviarVerificacion().code() == ACEPTADO }

    override suspend fun eliminarCuenta(contrasena: String): ResultadoEliminacion =
        protegido({ ResultadoEliminacion.Error(it) }) { eliminacion(api.eliminarCuenta(ConfirmacionContrasenaDto(contrasena))) }

    /** 200/201 traen `SesionApp`; el resto de códigos son los del contrato para registro e inicio de sesión. */
    private fun sesion(resp: Response<SesionAppDto>): ResultadoCuenta = when (resp.code()) {
        200, 201 -> resp.body()?.let { ResultadoCuenta.Exitoso(MapeadorSesion.aDominio(it)) }
            ?: ResultadoCuenta.Error(IllegalStateException("${resp.code()} sin cuerpo"))
        401 -> ResultadoCuenta.CredencialesInvalidas
        409 -> ResultadoCuenta.CorreoEnUso
        429 -> ResultadoCuenta.LimiteExcedido
        else -> ResultadoCuenta.Error(IllegalStateException("HTTP ${resp.code()}"))
    }

    private fun eliminacion(resp: Response<Unit>): ResultadoEliminacion = when (resp.code()) {
        204 -> ResultadoEliminacion.Eliminada
        403 -> ResultadoEliminacion.ConfirmacionInvalida
        else -> ResultadoEliminacion.Error(IllegalStateException("HTTP ${resp.code()}"))
    }

    /** Un fallo de red se vuelve el resultado de error del dominio; cancelar la corrutina se propaga. */
    private inline fun <T> protegido(enError: (Exception) -> T, accion: () -> T): T = try {
        accion()
    } catch (e: CancellationException) {
        throw e
    } catch (e: Exception) {
        enError(e)
    }

    private companion object {
        const val ACEPTADO = 202

        /** FR-061: la app siempre inicia sesión por el canal `app` (tokens en el cuerpo). */
        const val CANAL_APP = "app"
    }
}
