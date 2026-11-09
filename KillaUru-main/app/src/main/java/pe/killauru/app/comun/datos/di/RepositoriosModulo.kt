package pe.killauru.app.comun.datos.di

import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton
import pe.killauru.app.deteccion.dominio.RepositorioEventos
import pe.killauru.app.deteccion.datos.RepositorioEventosRoom
import pe.killauru.app.historial.dominio.RepositorioRecorridos
import pe.killauru.app.historial.datos.RepositorioRecorridosRoom
import pe.killauru.app.mapa.dominio.RepositorioAnomalias
import pe.killauru.app.mapa.datos.RepositorioAnomaliasRoom
import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import pe.killauru.app.sesion.datos.RepositorioPreferenciasDataStore

@Module
@InstallIn(SingletonComponent::class)
internal object RepositoriosModulo {
    @Provides fun eventos(impl: RepositorioEventosRoom): RepositorioEventos = impl
    @Provides fun recorridos(impl: RepositorioRecorridosRoom): RepositorioRecorridos = impl
    @Provides fun anomalias(impl: RepositorioAnomaliasRoom): RepositorioAnomalias = impl
    @Provides fun preferencias(impl: RepositorioPreferenciasDataStore): RepositorioPreferencias = impl
}
