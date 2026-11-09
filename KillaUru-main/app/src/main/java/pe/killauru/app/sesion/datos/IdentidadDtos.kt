package pe.killauru.app.sesion.datos

import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable

/** FR-063, FR-065: DTOs de la API de identidad. */
@Serializable
internal data class LoginRequestDto(
    @SerialName("correo") val correo: String,
    @SerialName("contrasena") val contrasena: String,
)

@Serializable
internal data class RegistroRequestDto(
    @SerialName("correo") val correo: String,
    @SerialName("contrasena") val contrasena: String,
)

@Serializable
internal data class TokenResponseDto(
    @SerialName("token") val token: String,
    @SerialName("expira") val expiraMs: Long,
    @SerialName("usuarioId") val usuarioId: String,
    @SerialName("permisoVenceMs") val permisoVenceMs: Long? = null,
)
