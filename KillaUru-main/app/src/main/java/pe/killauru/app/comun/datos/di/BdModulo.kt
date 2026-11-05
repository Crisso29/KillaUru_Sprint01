package pe.killauru.app.comun.datos.di

import android.content.Context
import androidx.room.Room
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.android.qualifiers.ApplicationContext
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton
import pe.killauru.app.comun.datos.bd.KillaUruBd
import pe.killauru.app.comun.datos.bd.Migraciones
import pe.killauru.app.comun.datos.bd.ContadorDao
import pe.killauru.app.comun.datos.bd.PurgaDao
import pe.killauru.app.comun.datos.bd.TriggersValidacion
import pe.killauru.app.deteccion.datos.EventoDao
import pe.killauru.app.historial.datos.RecorridoDao
import pe.killauru.app.mapa.datos.AnomaliaDao
import pe.killauru.app.mapa.datos.EstadoCopiaDao

/** T024: provee la base de datos Room y sus DAOs a través de Hilt. */
@Module
@InstallIn(SingletonComponent::class)
internal object BdModulo {
    @Provides
    @Singleton
    fun proveerBd(@ApplicationContext ctx: Context): KillaUruBd =
        Room.databaseBuilder(ctx, KillaUruBd::class.java, KillaUruBd.NOMBRE)
            .addMigrations(*Migraciones.TODAS)
            .addCallback(object : androidx.room.RoomDatabase.Callback() {
                override fun onCreate(db: androidx.sqlite.db.SupportSQLiteDatabase) {
                    TriggersValidacion.crear(db)
                }
            })
            .build()

    @Provides fun proveerPurgaDao(bd: KillaUruBd): PurgaDao = bd.purgaDao()
    @Provides fun proveerContadorDao(bd: KillaUruBd): ContadorDao = bd.contadorDao()
    @Provides fun proveerEventoDao(bd: KillaUruBd): EventoDao = bd.eventoDao()
    @Provides fun proveerAnomaliaDao(bd: KillaUruBd): AnomaliaDao = bd.anomaliaDao()
    @Provides fun proveerEstadoCopiaDao(bd: KillaUruBd): EstadoCopiaDao = bd.estadoCopiaDao()
    @Provides fun proveerRecorridoDao(bd: KillaUruBd): RecorridoDao = bd.recorridoDao()
}
