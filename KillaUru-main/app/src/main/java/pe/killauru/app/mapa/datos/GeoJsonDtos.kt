package pe.killauru.app.mapa.datos

import kotlinx.serialization.Serializable

/*
 * JSON del mapa con los nombres exactos de `mapa.openapi.yaml` (`ColeccionAnomalias`, `AnomaliaFeature`, `BajaFeature`).
 * Solo forma: la traducción al dominio está en MapeadorGeoJson. Fechas en ISO-8601 y catálogos como texto.
 */

@Serializable
internal data class ColeccionAnomaliasDto(
    val type: String,
    /** Hora del servidor; se guarda como próximo `updatedSince`. */
    val generadoUtc: String,
    val features: List<FeatureDto>,
)

/** `AnomaliaFeature` o `BajaFeature`: la baja trae `geometry: null` y `properties.eliminada = true`. */
@Serializable
internal data class FeatureDto(
    val type: String,
    val id: String,
    val geometry: GeometriaDto? = null,
    val properties: PropiedadesDto,
)

/** Punto GeoJSON: `coordinates` = `[longitud, latitud]`. */
@Serializable
internal data class GeometriaDto(val type: String, val coordinates: List<Double>)

/** Propiedades de ambas clases de feature; las de la anomalía son null en una baja. */
@Serializable
internal data class PropiedadesDto(
    val id: String,
    val kmVia: Double? = null,
    val tipo: String? = null,
    val severidad: Int? = null,
    val persistencia: String? = null,
    val ultimaDeteccionUtc: String? = null,
    /** null en los estados finales: la app lo trata como baja (R-011). */
    val venceUtc: String? = null,
    val eliminada: Boolean = false,
)
