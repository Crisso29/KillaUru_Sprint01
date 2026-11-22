package pe.killauru.app.mapa.datos.di

import dagger.Binds
import dagger.Module
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import pe.killauru.app.mapa.datos.RepositorioAnomaliasHttp
import pe.killauru.app.mapa.dominio.FuenteAnomalias
import javax.inject.Singleton

/**
 * FR-040, FR-044: inyección propia del módulo de mapa (Hilt, SingletonComponent).
 * Solo enlaza lo que es exclusivo del mapa. Los DAO están en `BdModulo` y el repositorio en `RepositoriosModulo`,
 * como en el resto de funcionalidades; repetirlos aquí hacía fallar Hilt con `DuplicateBindings`.
 */
@Module
@InstallIn(SingletonComponent::class)
internal abstract class MapaModulo {

    @Binds @Singleton
    abstract fun bindFuenteAnomalias(impl: RepositorioAnomaliasHttp): FuenteAnomalias
}
