package pe.killauru.app.mapa.datos

import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.Query

/** FR-040: endpoint delta de anomalías para actualización incremental. */
internal interface MapaApi {
    @GET("v1/mapa/anomalias")
    suspend fun anomalias(
        @Header("If-None-Match") etag: String? = null,
        @Query("desde") desdeUtc: Long? = null,
    ): Response<FeatureCollectionDto>
}
