package pe.killauru.app.sesion.datos

import kotlinx.serialization.Serializable
import pe.killauru.app.sesion.dominio.Sesion

@Serializable
internal data class SesionDto(
    val usuarioId: String,
    val correo: String,
    val rol: String,
    val tokenAcceso: String,
    val tokenRefresco: String,
    val venceUtc: Long,
) {
    companion object {
        fun de(sesion: Sesion) = SesionDto(
            usuarioId = sesion.usuarioId,
            correo = sesion.correo,
            rol = sesion.rol,
            tokenAcceso = sesion.tokenAcceso,
            tokenRefresco = sesion.tokenRefresco,
            venceUtc = sesion.venceUtc,
        )

        fun aSesion(dto: SesionDto) = Sesion(
            usuarioId = dto.usuarioId,
            correo = dto.correo,
            rol = dto.rol,
            tokenAcceso = dto.tokenAcceso,
            tokenRefresco = dto.tokenRefresco,
            venceUtc = dto.venceUtc,
        )
    }
}
