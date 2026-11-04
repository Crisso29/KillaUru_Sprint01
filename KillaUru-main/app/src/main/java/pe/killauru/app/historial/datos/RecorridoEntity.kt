package pe.killauru.app.historial.datos

import androidx.room.Entity
import androidx.room.Index
import androidx.room.PrimaryKey

/** data-model §1: resumen de cada viaje; solo con sesión; retención 6 meses. */
@Entity(
    tableName = "recorrido",
    indices = [Index("finUtc", name = "index_recorrido_finUtc")]
)
internal data class RecorridoEntity(
    @PrimaryKey(autoGenerate = true) val id: Long = 0,
    val inicioUtc: Long,
    val finUtc: Long,
    val kmDesde: Double,
    val kmHasta: Double,
    val kmRecorridos: Double,
    val eventos: Int,
)
