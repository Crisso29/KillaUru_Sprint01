package pe.killauru.app.mapa.datos

import okhttp3.ResponseBody
import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.Streaming
import retrofit2.http.Url

/** FR-042, FR-043: la CDN pública (`cdn.openapi.yaml`). Las URL son absolutas: las da el manifiesto. */
internal interface CdnApi {
    @GET
    suspend fun manifiesto(
        @Url url: String,
        @Header("If-None-Match") etag: String? = null,
    ): Response<ManifiestoMapaDto>

    /** `@Streaming`: el PMTiles se copia a disco por partes, sin cargarlo entero en memoria. */
    @Streaming
    @GET
    suspend fun descargar(
        @Url url: String,
        @Header("Range") rango: String? = null,
    ): Response<ResponseBody>
}
