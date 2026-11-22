package pe.killauru.app.sincronizacion.datos

import kotlinx.serialization.Serializable

/*
 * JSON del lote, con los nombres exactos de `ingesta.openapi.yaml` (esquemas `Lote`, `Evento` y `RespuestaLote`).
 * Solo forma: la traducción al dominio está en MapeadorLote. ContratoIngestaTest compara estos campos con el YAML.
 */

/** `Evento`: fechas en ISO-8601 UTC y `tipo` como texto del catálogo (`Bache`, `Hundimiento`). */
@Serializable
internal data class EventoDto(
    val eventoId: String,
    val detectadoUtc: String,
    val latitud: Double,
    val longitud: Double,
    val precisionMetros: Double,
    val tipo: String,
    val severidad: Int,
    val confidence: Double,
    val modeloVersion: String,
    val seudonimo: String,
)

/** `Lote`: sin `loteId`, porque lo genera el servidor (D3). */
@Serializable
internal data class LoteDto(val eventos: List<EventoDto>)

/** Elemento de `rechazados`: `id` es null si el evento no traía un `eventoId` válido (D5). */
@Serializable
internal data class RechazoDto(val id: String? = null, val indice: Int, val motivo: String)

/** `RespuestaLote` del 200: cada evento aparece en una sola lista. */
@Serializable
internal data class RespuestaLoteDto(
    val loteId: String,
    val aceptados: List<String>,
    val duplicados: List<String>,
    val rechazados: List<RechazoDto>,
)
