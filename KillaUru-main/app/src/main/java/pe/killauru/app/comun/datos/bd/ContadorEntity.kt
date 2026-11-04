package pe.killauru.app.comun.datos.bd

import androidx.room.Entity
import androidx.room.PrimaryKey

/** data-model §1: contadores locales de rechazos y expirados por motivo. */
@Entity(tableName = "contador_local")
internal data class ContadorEntity(
    @PrimaryKey val clave: String,
    val valor: Int,
)
