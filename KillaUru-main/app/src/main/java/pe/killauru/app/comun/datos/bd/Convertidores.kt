package pe.killauru.app.comun.datos.bd

import androidx.room.TypeConverter

/** data-model §4: códigos enteros estables; prueban CodigosEstablesTest. */
internal object Convertidores {
    // EstadoLocal: PENDING=0, ENVIANDO=1, SYNCED=2, REJECTED_LOCAL=3, EXPIRADO_LOCAL=4
    @TypeConverter fun deEstado(v: Int?): Int? = v
    @TypeConverter fun aEstado(v: Int?): Int? = v
}
