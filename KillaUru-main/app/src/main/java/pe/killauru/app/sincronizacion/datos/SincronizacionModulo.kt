package pe.killauru.app.sincronizacion.datos

import dagger.Binds
import dagger.Module
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import pe.killauru.app.sincronizacion.dominio.ApiLotes
import pe.killauru.app.sincronizacion.dominio.PurgadorEventos
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
internal abstract class SincronizacionModulo {
    @Binds @Singleton abstract fun bindApiLotes(impl: ClienteLotesHttp): ApiLotes
    @Binds @Singleton abstract fun bindPurgadorEventos(impl: PurgadorEventosRoom): PurgadorEventos
}
