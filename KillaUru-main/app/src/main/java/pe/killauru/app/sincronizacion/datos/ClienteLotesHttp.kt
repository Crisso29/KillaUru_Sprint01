package pe.killauru.app.sincronizacion.datos

import kotlinx.coroutines.CancellationException
import pe.killauru.app.comun.datos.red.LectorProblema
import pe.killauru.app.comun.datos.red.RetryAfter
import pe.killauru.app.sincronizacion.dominio.ApiLotes
import pe.killauru.app.sincronizacion.dominio.Lote
import pe.killauru.app.sincronizacion.dominio.RespuestaLoteApi
import retrofit2.Response
import javax.inject.Inject
import javax.inject.Singleton

/**
 * FR-021, FR-034, FR-035: envía un lote con el contrato 005 y traduce el código HTTP a [RespuestaLoteApi].
 * Los errores llevan el `codigo` del ProblemDetails (p. ej. `lote_invalido`) y el `Retry-After` del servidor.
 */
@Singleton
internal class ClienteLotesHttp @Inject constructor(
    private val api: IngestaApi,
) : ApiLotes {

    override suspend fun enviar(lote: Lote): RespuestaLoteApi = try {
        traducir(api.enviarLote(lote.seudonimo, MapeadorLote.aDto(lote)))
    } catch (e: CancellationException) {
        throw e // cancelar la corrutina no es un error de red: hay que propagarlo
    } catch (e: Exception) {
        RespuestaLoteApi.ErrorRed(e)
    }

    private fun traducir(resp: Response<RespuestaLoteDto>): RespuestaLoteApi = when (resp.code()) {
        200 -> resp.body()?.let { RespuestaLoteApi.Exitosa(MapeadorLote.aDominio(it)) }
            ?: RespuestaLoteApi.ErrorRed(IllegalStateException("200 sin cuerpo"))
        400 -> RespuestaLoteApi.Error400(LectorProblema.codigo(resp))
        401 -> RespuestaLoteApi.Error401(LectorProblema.codigo(resp))
        403 -> RespuestaLoteApi.Error403(LectorProblema.codigo(resp))
        413 -> RespuestaLoteApi.Error413(LectorProblema.codigo(resp))
        429 -> RespuestaLoteApi.Error429(reintentarEnMs(resp) ?: ESPERA_SIN_RETRY_AFTER_MS)
        503 -> RespuestaLoteApi.Error503(reintentarEnMs(resp))
        else -> RespuestaLoteApi.ErrorRed(IllegalStateException("HTTP ${resp.code()}"))
    }

    private fun reintentarEnMs(resp: Response<*>): Long? = RetryAfter.de(resp.raw())?.toMillis()

    private companion object {
        /** El contrato siempre manda `Retry-After` con el 429; si falta, se usa el primer escalón de FR-036 (30 s). */
        const val ESPERA_SIN_RETRY_AFTER_MS = 30_000L
    }
}
