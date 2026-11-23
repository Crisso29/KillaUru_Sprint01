package pe.killauru.app.mapa.dominio

/**
 * FR-043, FR-044: puerto hacia los archivos del mapa en el celular. La capa de datos decide cómo (carpeta privada,
 * descarga reanudable, SHA-256); el dominio solo decide **qué** y **cuándo**.
 */
internal interface ArchivosMapa {
    /** El manifiesto de la versión activa, o null si aún no hay mapa. */
    suspend fun instalado(): ManifiestoMapa?

    /** ETag del último manifiesto leído, para preguntar a la CDN «¿cambió algo?» (304). */
    suspend fun etag(): String?

    /** Descarga (o reanuda) un archivo y comprueba tamaño y SHA-256; [alProgresar] recibe los bytes recibidos en total. */
    suspend fun descargar(archivo: ArchivoMapa, alProgresar: (bytesRecibidos: Long) -> Unit): Boolean

    /** Deja activa esa versión (y su ETag) y borra los archivos de las anteriores. */
    suspend fun activar(manifiesto: ManifiestoMapa, etag: String?)
}

/** Resultado de comprobar y, si hace falta, descargar el mapa. */
internal sealed interface ResultadoMapa {
    data object AlDia : ResultadoMapa
    data class Instalado(val version: String) : ResultadoMapa

    /** Hay versión nueva pero no hay Wi-Fi y el conductor no permitió datos móviles; [bytes] es lo que pesa. */
    data class EsperandoWifi(val bytes: Long) : ResultadoMapa
    data class Fallo(val causa: Throwable) : ResultadoMapa
}
