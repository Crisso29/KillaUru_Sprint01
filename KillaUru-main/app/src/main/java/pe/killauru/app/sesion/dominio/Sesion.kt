package pe.killauru.app.sesion.dominio

/** FR-063: estado de sesión activa con token JWT y permisos. */
internal data class Sesion(
    val usuarioId: String,
    val token: String,
    val tokenExpiraMs: Long,
    val permiso: PermisoLocal?,
)
