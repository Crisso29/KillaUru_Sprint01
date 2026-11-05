package pe.killauru.app.mapa.datos

import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable

/** FR-040: DTOs GeoJSON para el delta de anomalías desde la API. */
@Serializable
internal data class FeatureCollectionDto(
    @SerialName("type") val type: String,
    @SerialName("features") val features: List<FeatureDto>,
)

@Serializable
internal data class FeatureDto(
    @SerialName("type") val type: String,
    @SerialName("geometry") val geometry: GeometryDto,
    @SerialName("properties") val properties: PropiedadesAnomaliaDto,
)

@Serializable
internal data class GeometryDto(
    @SerialName("type") val type: String,
    @SerialName("coordinates") val coordinates: List<Double>,
)

@Serializable
internal data class PropiedadesAnomaliaDto(
    @SerialName("id") val id: String,
    @SerialName("tipo") val tipo: Int,
    @SerialName("severidad") val severidad: Int,
    @SerialName("persistente") val persistente: Boolean = false,
    @SerialName("km") val km: Double,
    @SerialName("venceUtc") val venceUtc: Long? = null,
    @SerialName("ultimaDeteccionUtc") val ultimaDeteccionUtc: Long,
    @SerialName("enDemo") val enDemo: Boolean = false,
)
