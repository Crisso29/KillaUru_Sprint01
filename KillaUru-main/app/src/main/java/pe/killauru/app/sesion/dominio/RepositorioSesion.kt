package pe.killauru.app.sesion.dominio

/** FR-062, FR-063: puerto de persistencia de sesión cifrada. */
internal interface RepositorioSesion {
    suspend fun guardar(sesion: Sesion)
    suspend fun obtener(): Sesion?
    suspend fun borrar()
    suspend fun actualizarTokens(acceso: String, refresco: String, venceUtc: Long)
}
