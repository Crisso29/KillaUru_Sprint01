package pe.killauru.app.sesion.datos

import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.HTTP
import retrofit2.http.Header
import retrofit2.http.POST

/**
 * FR-060…FR-067, ADR-009: operaciones del contrato `specs/003-modulo-identidad-accesos/contracts/identidad.openapi.yaml`
 * que usa la app. Las rutas son las del contrato sin la barra inicial (la URL base termina en `/`).
 */
internal interface IdentidadApi {
    @POST("api/v1/auth/registro")
    suspend fun registrar(@Body solicitud: RegistroSolicitudDto): Response<SesionAppDto>

    @POST("api/v1/auth/inicio-sesion")
    suspend fun iniciarSesion(@Body solicitud: InicioSesionSolicitudDto): Response<SesionAppDto>

    @POST("api/v1/auth/renovar")
    suspend fun renovar(@Body solicitud: RenovarSolicitudDto): Response<SesionAppDto>

    /** Exige sesión: aquí el token va explícito porque lo usa la revocación pendiente, fuera del flujo normal. */
    @POST("api/v1/auth/cerrar-sesion")
    suspend fun cerrarSesion(@Header("Authorization") autorizacion: String): Response<Unit>

    @POST("api/v1/me/reenviar-verificacion")
    suspend fun reenviarVerificacion(): Response<Unit>

    /** `DELETE` con cuerpo: Retrofit lo exige declarado con `@HTTP(hasBody = true)`. */
    @HTTP(method = "DELETE", path = "api/v1/me", hasBody = true)
    suspend fun eliminarCuenta(@Body confirmacion: ConfirmacionContrasenaDto): Response<Unit>
}
