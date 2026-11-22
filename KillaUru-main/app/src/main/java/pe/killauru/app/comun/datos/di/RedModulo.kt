package pe.killauru.app.comun.datos.di

import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton
import kotlinx.serialization.json.Json
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import pe.killauru.app.comun.datos.red.ClienteHttp
import pe.killauru.app.comun.datos.red.NombresInterceptor
import okhttp3.Interceptor
import javax.inject.Named
import retrofit2.Retrofit
import retrofit2.converter.kotlinx.serialization.asConverterFactory

/**
 * T024, FR-062: provee el cliente HTTP único y Retrofit. Los interceptores los aporta cada funcionalidad con su
 * nombre (NombresInterceptor), así este módulo común no depende de sesión ni de seudónimo.
 */
@Module
@InstallIn(SingletonComponent::class)
internal object RedModulo {
    private const val BASE_URL = "https://api.killauru.pe/"

    @Provides
    @Singleton
    fun proveerOkHttp(
        @Named(NombresInterceptor.TOKEN) token: Interceptor,
        @Named(NombresInterceptor.SEUDONIMO) seudonimo: Interceptor,
        @Named(NombresInterceptor.RENOVADOR) renovador: Interceptor,
    ): OkHttpClient = ClienteHttp.construir(token, seudonimo, renovador)

    @Provides
    @Singleton
    fun proveerRetrofit(okHttp: OkHttpClient): Retrofit {
        val json = Json { ignoreUnknownKeys = true }
        return Retrofit.Builder()
            .baseUrl(BASE_URL)
            .client(okHttp)
            .addConverterFactory(json.asConverterFactory("application/json".toMediaType()))
            .build()
    }
}
