package pe.killauru.app.sesion.dominio

/** FR-063: contrato del repositorio de sesión cifrada. */
internal interface RepositorioSesion {
    suspend fun obtener(): Sesion?
    suspend fun guardar(sesion: Sesion)
    suspend fun cerrar()
}
