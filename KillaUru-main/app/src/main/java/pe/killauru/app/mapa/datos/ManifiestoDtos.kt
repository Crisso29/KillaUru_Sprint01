package pe.killauru.app.mapa.datos

import kotlinx.serialization.Serializable

/** Contrato `ArchivoMapa` de `cdn.openapi.yaml`. */
@Serializable
internal data class ArchivoMapaDto(
    val tipo: String,
    val version: String,
    val url: String,
    val bytes: Long,
    val sha256: String,
)

/** Contrato `ManifiestoMapa` de `cdn.openapi.yaml` (`GET /mapa/manifiesto.json`). */
@Serializable
internal data class ManifiestoMapaDto(
    val esquema: Int,
    val version: String,
    val publicadoUtc: String,
    val atribucion: String,
    val archivos: List<ArchivoMapaDto>,
)
