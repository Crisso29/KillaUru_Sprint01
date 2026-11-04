package pe.killauru.app.comun.datos.di

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.preferencesDataStore
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.android.qualifiers.ApplicationContext
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton
import pe.killauru.app.comun.datos.AnclaTiempoDataStore
import pe.killauru.app.comun.datos.RelojConfiableAndroid
import pe.killauru.app.comun.dominio.Reloj

private val Context.preferenciasStore: DataStore<Preferences> by preferencesDataStore("preferencias")

/** T024: provee el Reloj confiable a través de Hilt. */
@Module
@InstallIn(SingletonComponent::class)
internal object RelojModulo {
    @Provides
    @Singleton
    fun proveerDataStore(@ApplicationContext ctx: Context): DataStore<Preferences> =
        ctx.preferenciasStore

    @Provides
    @Singleton
    fun proveerAnclaTiempoDataStore(store: DataStore<Preferences>): AnclaTiempoDataStore =
        AnclaTiempoDataStore(store)

    @Provides
    @Singleton
    fun proveerReloj(reloj: RelojConfiableAndroid): Reloj = reloj
}
