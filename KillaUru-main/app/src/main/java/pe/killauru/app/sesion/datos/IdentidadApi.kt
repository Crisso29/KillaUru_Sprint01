package pe.killauru.app.sesion.datos

import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.POST
import retrofit2.http.Path

/** FR-063, FR-065, FR-067: endpoints de la API de identidad. */
internal interface IdentidadApi {
    @POST("v1/auth/register")
    suspend fun registrar(@Body req: RegistroRequestDto): Response<TokenResponseDto>

    @POST("v1/auth/login")
    suspend fun login(@Body req: LoginRequestDto): Response<TokenResponseDto>

    @POST("v1/auth/verify-email")
    suspend fun solicitarVerificacion(@Body req: Map<String, String>): Response<Unit>

    @DELETE("v1/users/{id}")
    suspend fun eliminar(@Path("id") id: String): Response<Unit>

    @POST("v1/auth/refresh")
    suspend fun renovarToken(@Body req: Map<String, String>): Response<TokenResponseDto>

    @POST("v1/auth/logout")
    suspend fun cerrarSesion(@Body req: Map<String, String>): Response<Unit>
}
