package pe.killauru.app.deteccion.datos

import androidx.room.Entity
import androidx.room.Index
import androidx.room.PrimaryKey

/** data-model §1: buffer de eventos pendientes de sincronización. */
@Entity(
    tableName = "evento",
    indices = [
        Index("estado", "detectadoUtc", name = "index_evento_estado_detectadoUtc"),
        Index("estado", "estadoDesde", name = "index_evento_estado_estadoDesde"),
    ]
)
internal data class EventoEntity(
    @PrimaryKey val eventoId: String,
    val detectadoUtc: Long,
    val lat: Double,
    val lon: Double,
    val precisionM: Double,
    val tipo: Int,
    val severidad: Int,
    val confidence: Double,
    val modeloVersion: String,
    val seudonimo: String,
    val estado: Int,
    val estadoDesde: Long,
)
