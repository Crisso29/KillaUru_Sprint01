package pe.killauru.app.soporte

import androidx.room.RoomDatabase

/**
 * Room en las pruebas: consultas, transacciones y el seguimiento de cambios corren en el hilo de la prueba. Con el
 * ejecutor por defecto (`arch_disk_io`) una consulta podía seguir viva al cerrar la base y el SQLite de Robolectric
 * fallaba («Illegal connection pointer») en la prueba **siguiente**, que no tenía nada que ver.
 */
internal fun <T : RoomDatabase> RoomDatabase.Builder<T>.sinHilos(): RoomDatabase.Builder<T> =
    allowMainThreadQueries()
        .setQueryExecutor { it.run() }
        .setTransactionExecutor { it.run() }
