package pe.killauru.app.mapa.datos

import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable

/**
 * FR-044: DTOs GeoJSON para el delta de anomalías desde la API.
 * Colección: `ColeccionAnomalias` del contrato `mapa.openapi.yaml`.
 */

@Serializable
internal data class FeatureCollectionDto(
    @SerialName("type") val type: String,
    /** Próximo `updatedSince`; milisegundos UTC serializado como epoch por el servidor. */
    @SerialName("generadoUtc") val generadoUtc: Long,
    @SerialName("features") val features: List<FeatureDto>,
)

/**
 * Un feature puede ser `AnomaliaFeature` o `BajaFeature`.
 * Se distinguen por `geometry == null` (baja) o `properties.eliminada == true` (baja explícita).
 */
@Serializable
internal data class FeatureDto(
    @SerialName("type") val type: String,
    /** null en `BajaFeature` (geometry: null del contrato). */
    @SerialName("geometry") val geometry: GeometryDto? = null,
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
    // AnomaliaFeature fields (null en BajaFeature)
    @SerialName("tipo") val tipo: Int? = null,
    @SerialName("severidad") val severidad: Int? = null,
    @SerialName("persistente") val persistente: Boolean = false,
    /** `kmVia` en el contrato, `km` en el dominio (D13). */
    @SerialName("km") val km: Double? = null,
    /** null en estados finales → se trata como baja (R-011). */
    @SerialName("venceUtc") val venceUtc: Long? = null,
    @SerialName("ultimaDeteccionUtc") val ultimaDeteccionUtc: Long? = null,
    @SerialName("enDemo") val enDemo: Boolean = false,
    // BajaFeature fields
    @SerialName("eliminada") val eliminada: Boolean = false,
)
