package pe.killauru.app.sesion.dominio

/** FR-062: sesión de usuario con tokens cifrados en Keystore. */
internal data class Sesion(
    val usuarioId: String,
    val correo: String,
    val rol: String,
    val tokenAcceso: String,
    val tokenRefresco: String,
    val venceUtc: Long,
)
