package pe.killauru.app.mapa.datos

import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable

/** FR-042: DTOs del manifiesto de mapa PMTiles desde CDN. */
@Serializable
internal data class ManifiestoMapaDto(
    @SerialName("version") val version: String,
    @SerialName("archivo") val archivo: String,
    @SerialName("sha256") val sha256: String,
    @SerialName("tamanoBytes") val tamanoBytes: Long,
    @SerialName("generadoUtc") val generadoUtc: Long,
)
