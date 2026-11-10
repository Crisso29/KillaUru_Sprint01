package pe.killauru.app.mapa.datos.di

import dagger.Binds
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import pe.killauru.app.comun.datos.bd.KillaUruBd
import pe.killauru.app.mapa.datos.AnomaliaDao
import pe.killauru.app.mapa.datos.EstadoCopiaDao
import pe.killauru.app.mapa.datos.RepositorioAnomaliasHttp
import pe.killauru.app.mapa.datos.RepositorioAnomaliasRoom
import pe.killauru.app.mapa.dominio.FuenteAnomalias
import pe.killauru.app.mapa.dominio.RepositorioAnomalias
import javax.inject.Singleton

/** FR-040, FR-044: inyección del módulo de mapa (Hilt, SingletonComponent). */
@Module
@InstallIn(SingletonComponent::class)
internal abstract class MapaModulo {

    @Binds @Singleton
    abstract fun bindRepositorioAnomalias(impl: RepositorioAnomaliasRoom): RepositorioAnomalias

    @Binds @Singleton
    abstract fun bindFuenteAnomalias(impl: RepositorioAnomaliasHttp): FuenteAnomalias

    companion object {
        @Provides
        fun provideAnomaliaDao(bd: KillaUruBd): AnomaliaDao = bd.anomaliaDao()

        @Provides
        fun provideEstadoCopiaDao(bd: KillaUruBd): EstadoCopiaDao = bd.estadoCopiaDao()
    }
}
