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

    /** HU-25: pide el enlace para fijar una contraseña nueva. */
    suspend fun recuperarContrasena(correo: String): ResultadoRecuperacion

    /** FR-068: los datos de la cuenta con sesión; null si no se pudieron obtener (sin red o sesión vencida). */
    suspend fun misDatos(): MisDatos?

    /**
     * FR-066, RF-38: revoca la sesión en el servidor. `true` si ya no queda nada por revocar (204, o 401 porque el
     * servidor no la reconoce); `false` si hay que reintentar (sin red, límite o error del servidor).
     */
    suspend fun revocarSesion(tokenAcceso: String, tokenRenovacion: String): Boolean
}
