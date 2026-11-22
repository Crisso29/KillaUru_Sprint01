package pe.killauru.app.sincronizacion.datos

import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.Header
import retrofit2.http.POST

/** FR-021: operación `recibirLote` del contrato `specs/005-modulo-ingesta/contracts/ingesta.openapi.yaml`. */
internal interface IngestaApi {
    /**
     * La ruta es la del contrato sin la barra inicial (la URL base termina en `/`).
     * La cabecera lleva el seudónimo **de este lote**, que puede no ser el actual si la app lo rotó (D2).
     */
    @POST("api/v1/anomalies/batch")
    suspend fun enviarLote(
        @Header("X-KillaUru-Seudonimo") seudonimo: String,
        @Body lote: LoteDto,
    ): Response<RespuestaLoteDto>
}
