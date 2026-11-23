package pe.killauru.app.mapa.datos

import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import kotlinx.serialization.json.Json
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import pe.killauru.app.BuildConfig
import pe.killauru.app.mapa.dominio.ArchivosMapa
import pe.killauru.app.mapa.dominio.CdnMapa
import pe.killauru.app.mapa.dominio.DescargaMapa
import pe.killauru.app.mapa.dominio.FuenteEstiloMapa
import pe.killauru.app.mapa.dominio.FuenteZonaDemo
import pe.killauru.app.corredor.datos.CargadorCorredor
import pe.killauru.app.corredor.datos.CargadorZonaDemo
import pe.killauru.app.corredor.dominio.FuenteCorredor
import retrofit2.Retrofit
import retrofit2.converter.kotlinx.serialization.asConverterFactory
import java.util.concurrent.TimeUnit
import javax.inject.Named
import javax.inject.Singleton

/**
 * FR-042, seguridad: la CDN pública tiene **su propio** cliente HTTP, sin los interceptores de la API. Con el cliente
 * compartido, la app enviaría el token de sesión y el seudónimo del conductor a `cdn.killauru.pe`, que no los necesita.
 */
@Module
@InstallIn(SingletonComponent::class)
internal object CdnModulo {
    const val URL_BASE = "cdn_url_base"
    private const val CONECTAR_S = 15L
    private const val LEER_S = 60L // un PMTiles grande en una red de montaña
    private val JSON = Json { ignoreUnknownKeys = true }

    @Provides @Singleton
    fun cdnApi(): CdnApi {
        val cliente = OkHttpClient.Builder()
            .connectTimeout(CONECTAR_S, TimeUnit.SECONDS)
            .readTimeout(LEER_S, TimeUnit.SECONDS)
            .build()
        return Retrofit.Builder()
            .baseUrl(BuildConfig.CDN_URL) // las URL de cada petición son absolutas; esta solo completa a Retrofit
            .client(cliente)
            .addConverterFactory(JSON.asConverterFactory("application/json".toMediaType()))
            .build()
            .create(CdnApi::class.java)
    }

    @Provides @Named(URL_BASE)
    fun urlBase(): String = BuildConfig.CDN_URL

    @Provides
    fun cdnMapa(impl: CdnMapaHttp): CdnMapa = impl

    @Provides
    fun archivosMapa(impl: ArchivosMapaLocal): ArchivosMapa = impl

    @Provides
    fun descargaMapa(impl: DescargaMapaWorkManager): DescargaMapa = impl

    @Provides
    fun zonaDemo(impl: CargadorZonaDemo): FuenteZonaDemo = impl

    @Provides
    fun corredor(impl: CargadorCorredor): FuenteCorredor = impl

    @Provides
    fun estiloMapa(impl: EstiloMapaLocal): FuenteEstiloMapa = impl
}
