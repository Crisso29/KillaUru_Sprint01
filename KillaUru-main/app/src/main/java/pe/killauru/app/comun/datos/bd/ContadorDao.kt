package pe.killauru.app.comun.datos.bd

import androidx.room.Dao
import androidx.room.Query
import androidx.room.Transaction

/**
 * data-model §1: contadores locales (`rechazo.<Motivo>`, `expirado_local`, `rechazado_local`).
 * Clase abstracta y no interfaz: Room solo admite métodos `@Transaction` con cuerpo en clases abstractas.
 */
@Dao
internal abstract class ContadorDao {

    /**
     * Suma [cantidad] al contador [clave], creándolo en 0 si no existe, en una sola transacción.
     * No se usa `INSERT … ON CONFLICT DO UPDATE` porque requiere SQLite 3.24 (Android 11) y `minSdk` es 26.
     * Tampoco `@Upsert`: reemplaza el valor en lugar de sumarlo.
     */
    @Transaction
    open suspend fun sumar(clave: String, cantidad: Int) {
        crearSiFalta(clave)
        aumentar(clave, cantidad)
    }

    @Query("SELECT valor FROM contador_local WHERE clave = :clave")
    abstract suspend fun obtener(clave: String): Int?

    @Query("SELECT * FROM contador_local")
    abstract suspend fun todos(): List<ContadorEntity>

    @Query("INSERT OR IGNORE INTO contador_local (clave, valor) VALUES (:clave, 0)")
    protected abstract suspend fun crearSiFalta(clave: String)

    @Query("UPDATE contador_local SET valor = valor + :cantidad WHERE clave = :clave")
    protected abstract suspend fun aumentar(clave: String, cantidad: Int)
}
