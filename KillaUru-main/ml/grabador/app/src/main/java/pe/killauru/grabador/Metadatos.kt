package pe.killauru.grabador

import org.json.JSONObject

/**
 * T019 / FR-004, FR-005 — metadatos de grabación sin campos personales (RN-18, S-06).
 * Solo se exporta JSON con alias, nunca IMEI, ANDROID_ID, nombres ni placas.
 */
internal data class Metadatos(
    val grabacionId: String,
    val recorrido: String,
    val grabadorVersion: String,
    val celularAlias: String,
    val celularMarcaModelo: String,
    val celularRamGb: Int,
    val celularAnioLanzamiento: Int,
    val celularAndroid: String,
    val vehiculoAlias: String,
    val soporteTipo: String,
    val soportePosicion: String,
    val tramoKmInicio: Double,
    val tramoKmFin: Double,
    val tramoSentido: String,
    val fecha: String,
    val esCelularVideo: Boolean,
) {
    /** Serializa a JSON sin ningún identificador del aparato (FR-005). */
    fun toJson(): String = JSONObject().apply {
        put("grabacionId", grabacionId)
        put("recorrido", recorrido)
        put("grabadorVersion", grabadorVersion)
        put("celular", JSONObject().apply {
            put("alias", celularAlias)
            put("marcaModelo", celularMarcaModelo)
            put("ramGb", celularRamGb)
            put("anioLanzamiento", celularAnioLanzamiento)
            put("android", celularAndroid)
        })
        put("vehiculo", JSONObject().apply {
            put("alias", vehiculoAlias)
            put("categoria", "Liviano")  // RN-18: solo Liviano
        })
        put("soporte", JSONObject().apply {
            put("tipo", soporteTipo)
            put("posicion", soportePosicion)
        })
        put("tramo", JSONObject().apply {
            put("kmInicio", tramoKmInicio)
            put("kmFin", tramoKmFin)
            put("sentido", tramoSentido)
        })
        put("fecha", fecha)
        put("esCelularVideo", esCelularVideo)
        put("parametros", JSONObject().apply {
            put("samplingPeriodUs", 20000)
            put("maxReportLatencyUs", 1000000)
            put("gpsIntervaloMs", 1000)
        })
    }.toString()
}
