package pe.killauru.app.mapa.datos

import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.Query

/** FR-044, FR-045: operación `consultarMapa` de `specs/006-modulo-consulta-mapa/contracts/mapa.openapi.yaml`. */
internal interface MapaApi {
    /**
     * @param etag valor del `ETag` previo para revalidar (304); solo sirve sin filtros.
     * @param updatedSince fecha ISO-8601 del último `generadoUtc`; null = descarga completa.
     */
    @GET("api/v1/anomalies")
    suspend fun anomalias(
        @Header("If-None-Match") etag: String? = null,
        @Query("updatedSince") updatedSince: String? = null,
    ): Response<ColeccionAnomaliasDto>
}
