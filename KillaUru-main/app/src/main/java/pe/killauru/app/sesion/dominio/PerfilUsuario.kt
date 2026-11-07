package pe.killauru.app.sesion.dominio

/** FR-064: datos del perfil de usuario obtenidos de /me. */
internal data class PerfilUsuario(
    val id: String,
    val correo: String,
    val nombre: String?,
    val rol: String,
    val verificado: Boolean,
)
