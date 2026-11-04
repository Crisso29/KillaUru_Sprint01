package pe.killauru.app.sesion.dominio

/** FR-060, FR-066: puerto de API de identidad (registro, login, perfil, etc.). */
internal interface ApiCuenta {
    suspend fun registrar(correo: String, contrasena: String): ResultadoCuenta
    suspend fun iniciarSesion(correo: String, contrasena: String): Result<Sesion>
    suspend fun renovarToken(refresco: String): Result<Pair<String, Long>>
    suspend fun cerrarSesion(acceso: String): ResultadoCuenta
    suspend fun eliminarCuenta(acceso: String, contrasena: String): ResultadoCuenta
    suspend fun obtenerPerfil(acceso: String): Result<PerfilUsuario>
    suspend fun reenviarVerificacion(correo: String): ResultadoCuenta
}
