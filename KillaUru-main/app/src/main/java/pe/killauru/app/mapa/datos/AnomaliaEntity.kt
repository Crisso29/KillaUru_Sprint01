package pe.killauru.app.mapa.datos

import androidx.room.Entity
import androidx.room.Index
import androidx.room.PrimaryKey

/** data-model §1: copia local de anomalías para mapa y alertas offline. */
@Entity(
    tableName = "anomalia",
    indices = [Index("km", name = "index_anomalia_km")]
)
internal data class AnomaliaEntity(
    @PrimaryKey val id: String,
    val tipo: Int,
    val severidad: Int,
    val persistente: Int,
    val lat: Double,
    val lon: Double,
    val km: Double,
    val venceUtc: Long,
    val ultimaDeteccionUtc: Long,
    val enDemo: Int,
)
