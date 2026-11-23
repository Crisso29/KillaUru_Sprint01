package pe.killauru.app.mapa.dominio

/** Los 4 archivos del mapa que publica la CDN (contrato `ArchivoMapa.tipo`). */
internal enum class TipoArchivoMapa { PMTILES, CORREDOR, TRAMO_DEMO, ESTILO }

/** FR-043: un archivo versionado del mapa; se descarga y se comprueba con [bytes] y [sha256]. */
internal data class ArchivoMapa(
    val tipo: TipoArchivoMapa,
    val version: String,
    val url: String,
    val bytes: Long,
    val sha256: String,
) {
    /** Nombre del archivo en la CDN y en el celular, p. ej. `corredor-pe28a.v3.pmtiles`. */
    val nombre: String get() = url.substringAfterLast('/')
}

/**
 * FR-042, FR-043 (contrato `ManifiestoMapa` de `cdn.openapi.yaml`): versión vigente de los datos del mapa.
 * [atribucion] se muestra siempre sobre el mapa (ADR-011).
 */
internal data class ManifiestoMapa(
    val version: String,
    val publicadoUtcMs: Long,
    val atribucion: String,
    val archivos: List<ArchivoMapa>,
) {
    fun archivo(tipo: TipoArchivoMapa): ArchivoMapa = archivos.first { it.tipo == tipo }

    /** Lo que ocupa todo junto: lo que se muestra como «38 MB» y lo que mide el progreso. */
    val bytesTotales: Long get() = archivos.sumOf { it.bytes }
}
