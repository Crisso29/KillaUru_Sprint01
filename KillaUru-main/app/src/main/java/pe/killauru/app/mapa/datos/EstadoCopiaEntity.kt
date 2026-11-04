package pe.killauru.app.mapa.datos

import androidx.room.Entity
import androidx.room.PrimaryKey

/** data-model §1: una sola fila; estado de la última descarga del mapa. */
@Entity(tableName = "estado_copia")
internal data class EstadoCopiaEntity(
    @PrimaryKey val id: Int = 1,
    val alcance: Int,
    val generadoUtc: Long?,
    val etag: String?,
    val verificadoUtc: Long?,
)
