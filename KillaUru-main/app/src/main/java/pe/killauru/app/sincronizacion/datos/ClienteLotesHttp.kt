package pe.killauru.app.sincronizacion.datos

import pe.killauru.app.sincronizacion.dominio.ApiLotes
import pe.killauru.app.sincronizacion.dominio.Lote
import pe.killauru.app.sincronizacion.dominio.RespuestaLoteApi
import pe.killauru.app.sincronizacion.dominio.RespuestaLoteDto
import java.time.Instant
import javax.inject.Inject
import javax.inject.Singleton

/** FR-021, FR-034: implementación HTTP de ApiLotes vía Retrofit. */
@Singleton
internal class ClienteLotesHttp @Inject constructor(
    private val api: IngestaApi,
) : ApiLotes {

    override suspend fun enviar(lote: Lote): RespuestaLoteApi = try {
        val request = LoteRequestDto(
            seudonimo = lote.seudonimo,
            eventos = lote.eventos.map { e ->
                EventoDto(
                    eventoId = e.eventoId,
                    detectadoUtc = Instant.ofEpochMilli(e.detectadoUtc.toEpochMilli()).toString(),
                    lat = e.lat,
                    lon = e.lon,
                    precisionM = e.precisionM,
                    tipo = e.tipo.codigo,
                    severidad = e.severidad,
                    confidence = e.confidence,
                    modeloVersion = e.modeloVersion,
                )
            },
        )
        val resp = api.enviarLote(request)
        when (resp.code()) {
            200 -> {
                val body = resp.body()!!
                RespuestaLoteApi.Exitosa(
                    RespuestaLoteDto(
                        aceptados = body.aceptados,
                        rechazados = body.rechazados.mapNotNull { it.id },
                        pendientes = body.pendientes,
                    )
                )
            }
            400 -> RespuestaLoteApi.Error400(resp.code().toString())
            401 -> RespuestaLoteApi.Error401(resp.code().toString())
            403 -> RespuestaLoteApi.Error403(resp.code().toString())
            413 -> RespuestaLoteApi.Error413(resp.code().toString())
            429 -> RespuestaLoteApi.Error429(60_000L)
            503 -> RespuestaLoteApi.Error503(null)
            else -> RespuestaLoteApi.ErrorRed(RuntimeException("HTTP ${resp.code()}"))
        }
    } catch (e: Exception) {
        RespuestaLoteApi.ErrorRed(e)
    }
}
