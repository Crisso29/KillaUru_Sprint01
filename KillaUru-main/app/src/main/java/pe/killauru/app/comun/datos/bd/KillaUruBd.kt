package pe.killauru.app.comun.datos.bd

import androidx.room.Database
import androidx.room.RoomDatabase
import pe.killauru.app.deteccion.datos.EventoDao
import pe.killauru.app.deteccion.datos.EventoEntity
import pe.killauru.app.historial.datos.RecorridoDao
import pe.killauru.app.historial.datos.RecorridoEntity
import pe.killauru.app.mapa.datos.AnomaliaDao
import pe.killauru.app.mapa.datos.AnomaliaEntity
import pe.killauru.app.mapa.datos.EstadoCopiaDao
import pe.killauru.app.mapa.datos.EstadoCopiaEntity

/** data-model §1, R-005: versión 1; WAL; exportSchema para migraciones seguras. */
@Database(
    entities = [
        EventoEntity::class,
        AnomaliaEntity::class,
        EstadoCopiaEntity::class,
        RecorridoEntity::class,
        ContadorEntity::class,
    ],
    version = 1,
    exportSchema = true,
)
internal abstract class KillaUruBd : RoomDatabase() {
    abstract fun purgaDao(): PurgaDao
    abstract fun contadorDao(): ContadorDao
    abstract fun eventoDao(): EventoDao
    abstract fun anomaliaDao(): AnomaliaDao
    abstract fun estadoCopiaDao(): EstadoCopiaDao
    abstract fun recorridoDao(): RecorridoDao

    companion object {
        /** Nombre fijo del archivo SQLite (R-027: fuera del respaldo de Android). */
        const val NOMBRE = "killauru.db"
    }
}
