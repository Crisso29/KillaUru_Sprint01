package pe.killauru.app.deteccion.dominio

import java.time.Instant

/** FR-013, FR-019, data-model §2: evento de anomalía detectado por el modelo. */
internal data class Evento(
    val eventoId: String,
    val detectadoUtc: Instant,
    val lat: Double,
    val lon: Double,
    val precisionM: Double,
    val tipo: TipoAnomalia,
    val severidad: Int,
    val confidence: Double,
    val modeloVersion: String,
    val seudonimo: String,
    val estado: EstadoLocal,
    val estadoDesde: Instant,
)
