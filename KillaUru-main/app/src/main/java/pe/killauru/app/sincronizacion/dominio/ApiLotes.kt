package pe.killauru.app.sincronizacion.dominio

internal interface ApiLotes {
    suspend fun enviar(lote: Lote): RespuestaLoteApi
}

internal sealed class RespuestaLoteApi {
    data class Exitosa(val dto: RespuestaLoteDto) : RespuestaLoteApi()
    data class Error400(val codigo: String?) : RespuestaLoteApi()
    data class Error401(val codigo: String?) : RespuestaLoteApi()
    data class Error403(val codigo: String?) : RespuestaLoteApi()
    data class Error413(val codigo: String?) : RespuestaLoteApi()
    data class Error429(val reintentarEn: Long) : RespuestaLoteApi()
    data class Error503(val reintentarEn: Long?) : RespuestaLoteApi()
    data class ErrorRed(val causa: Throwable) : RespuestaLoteApi()
}
