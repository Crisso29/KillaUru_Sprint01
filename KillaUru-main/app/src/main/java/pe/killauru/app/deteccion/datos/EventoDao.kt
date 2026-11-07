package pe.killauru.app.deteccion.datos

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import androidx.room.Transaction
import kotlinx.coroutines.flow.Flow

@Dao
internal interface EventoDao {
    @Insert(onConflict = OnConflictStrategy.IGNORE)
    suspend fun insertar(evento: EventoEntity)

    @Query("SELECT * FROM evento WHERE estado = 0 AND seudonimo = :seudonimo ORDER BY detectadoUtc, eventoId LIMIT :max")
    suspend fun tomarLote(seudonimo: String, max: Int): List<EventoEntity>

    @Query("SELECT * FROM evento WHERE estado = 0 ORDER BY detectadoUtc")
    fun pendientesFlow(): Flow<List<EventoEntity>>

    @Query("SELECT COUNT(*) FROM evento WHERE estado = 0")
    suspend fun contarPendientes(): Int

    @Query("SELECT COUNT(*) FROM evento WHERE estado = 1")
    suspend fun contarEnviando(): Int

    @Query("UPDATE evento SET estado = 0, estadoDesde = :ahoraMs WHERE estado = 1")
    suspend fun recuperarEnviando(ahoraMs: Long)

    @Query("UPDATE evento SET estado = 4, estadoDesde = :ahoraMs WHERE estado IN (0,1) AND detectadoUtc < :limiteMs")
    suspend fun expirar(limiteMs: Long, ahoraMs: Long)

    @Transaction
    suspend fun aplicarEstados(cambios: Map<String, Int>, ahoraMs: Long) {
        for ((id, estado) in cambios) {
            actualizarEstado(id, estado, ahoraMs)
        }
    }

    @Query("UPDATE evento SET estado = :estado, estadoDesde = :ahoraMs WHERE eventoId = :id")
    suspend fun actualizarEstado(id: String, estado: Int, ahoraMs: Long)
}
