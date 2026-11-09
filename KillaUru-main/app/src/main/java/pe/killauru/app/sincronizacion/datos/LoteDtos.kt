package pe.killauru.app.sincronizacion.datos

import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable

@Serializable
internal data class EventoDto(
    @SerialName("eventoId") val eventoId: String,
    @SerialName("detectadoUtc") val detectadoUtc: String,
    @SerialName("lat") val lat: Double,
    @SerialName("lon") val lon: Double,
    @SerialName("precisionM") val precisionM: Double,
    @SerialName("tipo") val tipo: Int,
    @SerialName("severidad") val severidad: Int,
    @SerialName("confidence") val confidence: Double,
    @SerialName("modeloVersion") val modeloVersion: String,
)

@Serializable
internal data class LoteRequestDto(
    @SerialName("seudonimo") val seudonimo: String,
    @SerialName("eventos") val eventos: List<EventoDto>,
)

@Serializable
internal data class RespuestaRechazoDto(
    @SerialName("id") val id: String? = null,
    @SerialName("indice") val indice: Int? = null,
    @SerialName("motivo") val motivo: String? = null,
)

@Serializable
internal data class LoteResponseDto(
    @SerialName("aceptados") val aceptados: List<String> = emptyList(),
    @SerialName("rechazados") val rechazados: List<RespuestaRechazoDto> = emptyList(),
    @SerialName("pendientes") val pendientes: List<String> = emptyList(),
)
