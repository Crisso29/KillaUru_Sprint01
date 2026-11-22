package pe.killauru.app.seudonimo.datos

import dagger.Binds
import dagger.Module
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import okhttp3.Interceptor
import pe.killauru.app.comun.datos.red.NombresInterceptor
import pe.killauru.app.seudonimo.dominio.RepositorioSeudonimo
import javax.inject.Named

/** FR-039, FR-040: el puerto del seudónimo y su interceptor para las peticiones anónimas. */
@Module
@InstallIn(SingletonComponent::class)
internal abstract class SeudonimoModulo {
    @Binds
    abstract fun bindRepositorioSeudonimo(impl: RepositorioSeudonimoDataStore): RepositorioSeudonimo

    @Binds
    @Named(NombresInterceptor.SEUDONIMO)
    abstract fun bindInterceptorSeudonimo(impl: InterceptorSeudonimo): Interceptor
}
