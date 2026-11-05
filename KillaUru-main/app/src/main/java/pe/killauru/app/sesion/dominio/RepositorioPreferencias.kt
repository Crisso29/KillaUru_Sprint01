package pe.killauru.app.sesion.dominio

/** FR-003, FR-005: acceso a las preferencias del usuario sin dependencias Android. */
internal interface RepositorioPreferencias {
    suspend fun obtenerTerminosVersion(): String?
    suspend fun guardarTerminosVersion(version: String)
    suspend fun obtenerTerminosAceptadoUtc(): Long?
    suspend fun guardarTerminosAceptadoUtc(utc: Long)
    suspend fun obtenerPoliticaVersion(): String?
    suspend fun guardarPoliticaVersion(version: String)
    suspend fun obtenerPoliticaAceptadoUtc(): Long?
    suspend fun guardarPoliticaAceptadoUtc(utc: Long)
    suspend fun obtenerCategoria(): CategoriaVehiculo?
    suspend fun guardarCategoria(categoria: CategoriaVehiculo)
    suspend fun obtenerSoloVerMapa(): Boolean
    suspend fun guardarSoloVerMapa(valor: Boolean)
    suspend fun obtenerUsarDatosMoviles(): Boolean
    suspend fun guardarUsarDatosMoviles(valor: Boolean)
    suspend fun obtenerSeudonimo(): String?
    suspend fun guardarSeudonimo(seudonimo: String, desdeUtc: Long)
    suspend fun obtenerSeudonimoDesdeUtc(): Long?
    suspend fun obtenerBackoffPaso(): Int
    suspend fun guardarBackoffPaso(paso: Int)
    suspend fun obtenerMapaVersion(): String?
    suspend fun guardarMapaVersion(version: String)
    suspend fun obtenerModeloVersion(): String?
    suspend fun guardarModeloVersion(version: String)
    suspend fun limpiarSesion()
}
