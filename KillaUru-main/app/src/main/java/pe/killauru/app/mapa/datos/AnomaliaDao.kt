package pe.killauru.app.mapa.datos

import androidx.room.Dao
import androidx.room.Query
import androidx.room.Upsert

/**
 * FR-040: acceso a la tabla `anomalia`.
 * Usa `@Upsert` para que la misma entidad pueda ser alta o actualización (D13).
 */
@Dao
internal interface AnomaliaDao {

    @Upsert
    suspend fun upsert(anomalias: List<AnomaliaEntity>)

    /** Borra un lote de ids de un golpe (eficiencia en deltas). */
    @Query("DELETE FROM anomalia WHERE id IN (:ids)")
    suspend fun borrar(ids: List<String>)

    @Query("SELECT * FROM anomalia WHERE venceUtc > :ahoraMs OR persistente = 1")
    suspend fun vigentes(ahoraMs: Long): List<AnomaliaEntity>

    /** Mantiene solo las que están dentro de la zona demo. */
    @Query("DELETE FROM anomalia WHERE enDemo = 0")
    suspend fun recortarAlDemo()

    /** Limpia toda la copia (cambio de rol). */
    @Query("DELETE FROM anomalia")
    suspend fun borrarTodas()
}
