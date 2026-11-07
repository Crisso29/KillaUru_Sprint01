package pe.killauru.app.mapa.dominio

/** FR-042, FR-043: manifiesto descargado desde CDN con metadatos del tile set. */
internal data class ManifiestoMapa(
    val version: String,
    val archivo: String,
    val sha256: String,
    val tamanoBytes: Long,
    val generadoUtc: Long,
    val etag: String?,
)
