package pe.killauru.app.mapa.datos

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import androidx.room.Transaction

@Dao
internal interface AnomaliaDao {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertar(anomalia: AnomaliaEntity)

    @Query("DELETE FROM anomalia WHERE id = :id")
    suspend fun eliminar(id: String)

    @Query("SELECT * FROM anomalia WHERE venceUtc > :ahoraMs OR persistente = 1")
    suspend fun vigentes(ahoraMs: Long): List<AnomaliaEntity>

    @Query("DELETE FROM anomalia WHERE enDemo = 0")
    suspend fun recortarAlDemo()

    @Transaction
    suspend fun actualizarLote(altas: List<AnomaliaEntity>, bajas: List<String>) {
        for (alta in altas) insertar(alta)
        for (id in bajas) eliminar(id)
    }
}
