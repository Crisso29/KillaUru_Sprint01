package pe.killauru.app.sincronizacion.dominio

internal data class RechazoDto(val id: String?, val indice: Int?)

internal data class RespuestaLoteDto(
    val aceptados: List<String>,
    val rechazados: List<RechazoDto>,
    val pendientes: List<String>,
)
