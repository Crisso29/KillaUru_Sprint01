package pe.killauru.app.historial.datos

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query

@Dao
internal interface RecorridoDao {
    @Insert(onConflict = OnConflictStrategy.IGNORE)
    suspend fun insertar(recorrido: RecorridoEntity)

    @Query("SELECT * FROM recorrido ORDER BY finUtc DESC")
    suspend fun todos(): List<RecorridoEntity>

    @Query("DELETE FROM recorrido WHERE finUtc < :limiteMs")
    suspend fun purgar(limiteMs: Long)
}
