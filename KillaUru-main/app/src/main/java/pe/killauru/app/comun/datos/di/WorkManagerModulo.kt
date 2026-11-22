package pe.killauru.app.comun.datos.di

import android.content.Context
import androidx.work.WorkManager
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.android.qualifiers.ApplicationContext
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

/**
 * FR-028: entrega `WorkManager` a quien lo pida por constructor (p. ej. `DescargaViewModel`), así se puede
 * sustituir por un doble en las pruebas. La app usa la configuración bajo demanda (`KillaUruApp` implementa
 * `Configuration.Provider`), por lo que `getInstance` la inicializa con esa configuración la primera vez.
 */
@Module
@InstallIn(SingletonComponent::class)
internal object WorkManagerModulo {
    @Provides
    @Singleton
    fun proveerWorkManager(@ApplicationContext ctx: Context): WorkManager = WorkManager.getInstance(ctx)
}
