package pe.killauru.app.comun.datos.bd

import androidx.room.Dao
import androidx.room.Query

/** data-model §5: purgas diarias de estados finales, recorridos y anomalías vencidas. */
@Dao
internal interface PurgaDao {
    @Query("DELETE FROM evento WHERE estado IN (2,3,4) AND estadoDesde < :ahoraMenos7d")
    suspend fun purgarEventosFinales(ahoraMenos7d: Long): Int

    @Query("DELETE FROM recorrido WHERE finUtc < :ahoraMenos6Meses")
    suspend fun purgarRecorridos(ahoraMenos6Meses: Long): Int

    @Query("DELETE FROM anomalia WHERE venceUtc < :ahoraMenos7d")
    suspend fun purgarAnomaliasVencidas(ahoraMenos7d: Long): Int
}
