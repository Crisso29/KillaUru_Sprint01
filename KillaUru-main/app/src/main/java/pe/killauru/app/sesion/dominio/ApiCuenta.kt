package pe.killauru.app.sesion.dominio

/** FR-063, FR-065, FR-067: contrato del API de identidad del servidor. */
internal interface ApiCuenta {
    suspend fun registrar(correo: String, contrasena: String): ResultadoCuenta
    suspend fun login(correo: String, contrasena: String): ResultadoCuenta
    suspend fun solicitarVerificacion(correo: String)
    suspend fun eliminarCuenta(usuarioId: String, token: String): Boolean
}
