package pe.killauru.app.sesion.datos

import dagger.Binds
import dagger.Module
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.RepositorioSesion
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
}
