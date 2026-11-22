package pe.killauru.app.sesion.dominio

/**
 * FR-066, R-023: si al cerrar sesión no hubo red, la revocación en el servidor queda para cuando la haya. El dominio
 * solo la pide; cómo se guarda (cifrada) y cuándo se envía lo decide la capa de datos.
 */
internal interface RevocacionPendiente {
    suspend fun programar(tokenRenovacion: String)
}
