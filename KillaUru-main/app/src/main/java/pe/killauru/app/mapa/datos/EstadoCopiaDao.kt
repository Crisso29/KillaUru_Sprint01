package pe.killauru.app.mapa.datos

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query

@Dao
internal interface EstadoCopiaDao {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun guardar(estado: EstadoCopiaEntity)

    @Query("SELECT * FROM estado_copia WHERE id = 1")
    suspend fun obtener(): EstadoCopiaEntity?

    @Query("DELETE FROM estado_copia WHERE id = 1")
    suspend fun limpiar()
}
