package pe.killauru.app.mapa.datos

import okhttp3.ResponseBody
import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.Url

/** FR-042, FR-043: endpoints CDN para manifiesto y tiles PMTiles. */
internal interface CdnApi {
    @GET
    suspend fun manifiesto(@Url url: String): Response<ManifiestoMapaDto>

    @GET
    suspend fun descargar(
        @Url url: String,
        @Header("Range") rango: String? = null,
    ): Response<ResponseBody>
}
