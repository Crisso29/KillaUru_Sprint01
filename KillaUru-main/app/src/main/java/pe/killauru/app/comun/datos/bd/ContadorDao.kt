package pe.killauru.app.comun.datos.bd

import androidx.room.Dao
import androidx.room.Query
import androidx.room.Upsert

/** data-model §1: contadores de rechazos por motivo y expirados locales. */
@Dao
internal interface ContadorDao {
    @Upsert
    suspend fun incrementar(entidad: ContadorEntity)

    @Query("SELECT valor FROM contador_local WHERE clave = :clave")
    suspend fun obtener(clave: String): Int?

    @Query("SELECT * FROM contador_local")
    suspend fun todos(): List<ContadorEntity>
}
