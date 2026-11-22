package pe.killauru.app.sesion.datos

import dagger.Binds
import dagger.Module
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.RepositorioSesion
import okhttp3.Interceptor
import pe.killauru.app.comun.datos.red.NombresInterceptor
import javax.inject.Named
import javax.inject.Singleton

/** Bindings Hilt para el módulo de sesión (FR-063, FR-065, FR-067). */
@Module
@InstallIn(SingletonComponent::class)
internal abstract class SesionModulo {

    @Binds
    @Singleton
    abstract fun bindApiCuenta(impl: ApiCuentaHttp): ApiCuenta

    @Binds
    @Singleton
    abstract fun bindRepositorioSesion(impl: RepositorioSesionCifrado): RepositorioSesion

    /** FR-062: el token de acceso y su renovación entran al cliente HTTP común por nombre (RedModulo). */
    @Binds
    @Named(NombresInterceptor.TOKEN)
    abstract fun bindInterceptorToken(impl: InterceptorToken): Interceptor

    @Binds
    @Named(NombresInterceptor.RENOVADOR)
    abstract fun bindRenovadorToken(impl: RenovadorToken): Interceptor
}
