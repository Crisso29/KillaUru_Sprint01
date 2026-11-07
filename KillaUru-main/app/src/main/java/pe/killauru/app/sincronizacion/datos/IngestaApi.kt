package pe.killauru.app.sincronizacion.datos

import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST

/** FR-021: endpoint REST para ingestión de lotes de anomalías. */
internal interface IngestaApi {
    @POST("v1/ingest/batch")
    suspend fun enviarLote(@Body lote: LoteRequestDto): Response<LoteResponseDto>
}
