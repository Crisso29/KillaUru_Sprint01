package pe.killauru.app.sesion.datos

/** R-023: dónde espera el token de renovación de una sesión cerrada sin red, hasta revocarla en el servidor. */
internal interface AlmacenRevocacion {
    suspend fun guardar(tokenRenovacion: String)
    suspend fun obtener(): String?
    suspend fun borrar()
}
