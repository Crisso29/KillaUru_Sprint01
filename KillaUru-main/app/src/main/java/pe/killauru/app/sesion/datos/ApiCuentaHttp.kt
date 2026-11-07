package pe.killauru.app.sesion.datos

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.PermisoLocal
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import pe.killauru.app.sesion.dominio.Sesion
import javax.inject.Inject
import javax.inject.Singleton

/** FR-063, FR-065, FR-067: implementación HTTP de ApiCuenta. */
@Singleton
internal class ApiCuentaHttp @Inject constructor(
    private val api: IdentidadApi,
) : ApiCuenta {

    override suspend fun registrar(correo: String, contrasena: String): ResultadoCuenta = try {
        val resp = api.registrar(RegistroRequestDto(correo, contrasena))
        mapearRespuesta(resp)
    } catch (e: Exception) { ResultadoCuenta.Error(e) }

    override suspend fun login(correo: String, contrasena: String): ResultadoCuenta = try {
        val resp = api.login(LoginRequestDto(correo, contrasena))
        mapearRespuesta(resp)
    } catch (e: Exception) { ResultadoCuenta.Error(e) }

    override suspend fun solicitarVerificacion(correo: String) {
        try { api.solicitarVerificacion(mapOf("correo" to correo)) } catch (_: Exception) {}
    }

    override suspend fun eliminarCuenta(usuarioId: String, token: String): Boolean = try {
        api.eliminar(usuarioId).isSuccessful
    } catch (_: Exception) { false }

    private fun mapearRespuesta(resp: retrofit2.Response<TokenResponseDto>): ResultadoCuenta =
        when (resp.code()) {
            200, 201 -> {
                val body = resp.body()!!
                val permiso = body.permisoVenceMs?.let { PermisoLocal(it) }
                ResultadoCuenta.Exitoso(Sesion(body.usuarioId, body.token, body.expiraMs, permiso))
            }
            401 -> ResultadoCuenta.CredencialesInvalidas
            403 -> ResultadoCuenta.CorreoNoVerificado
            409 -> ResultadoCuenta.CorreoYaRegistrado
            else -> ResultadoCuenta.Error(RuntimeException("HTTP ${resp.code()}"))
        }
}
