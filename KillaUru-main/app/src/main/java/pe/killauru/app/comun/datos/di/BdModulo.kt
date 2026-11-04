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
import pe.killauru.app.comun.datos.bd.PurgaDao
import pe.killauru.app.comun.datos.bd.ContadorDao
import pe.killauru.app.comun.datos.bd.TriggersValidacion

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
}
