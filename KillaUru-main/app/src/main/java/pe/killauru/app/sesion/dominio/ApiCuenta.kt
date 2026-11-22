package pe.killauru.app.sesion.dominio

/**
 * FR-060…FR-067: operaciones de cuenta del contrato 003 que usa la app.
 * Las que exigen sesión no reciben el token: lo añade el cliente HTTP, así el dominio no maneja credenciales.
 */
internal interface ApiCuenta {
    suspend fun registrar(solicitud: SolicitudRegistro): ResultadoCuenta
    suspend fun iniciarSesion(correo: String, contrasena: String): ResultadoCuenta

    /** FR-064: `true` si el servidor aceptó reenviar el correo de verificación. */
    suspend fun reenviarVerificacion(): Boolean

    suspend fun eliminarCuenta(contrasena: String): ResultadoEliminacion
}
