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
import pe.killauru.app.comun.datos.red.InterceptorSeudonimo
import retrofit2.Retrofit
import retrofit2.converter.kotlinx.serialization.asConverterFactory

/** T024: provee OkHttp y Retrofit con el interceptor de seudónimo. */
@Module
@InstallIn(SingletonComponent::class)
internal object RedModulo {
    private const val BASE_URL = "https://api.killauru.pe/"

    @Provides
    @Singleton
    fun proveerInterceptor(): InterceptorSeudonimo =
        InterceptorSeudonimo { null }  // sobreescrito en SesionModulo al inyectar el seudonimo

    @Provides
    @Singleton
    fun proveerOkHttp(interceptor: InterceptorSeudonimo): OkHttpClient =
        ClienteHttp.construir(interceptor)

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
