package pe.killauru.app.sincronizacion.dominio

/**
 * FR-031, D5 y data-model (transiciones de `evento`): decide el estado local de cada evento enviado según el 200.
 *
 * | Caso en la respuesta | Estado |
 * |---|---|
 * | `aceptados`, `duplicados` o `rechazados` **con** `id` | SYNCED: el servidor ya respondió por él y no se reenvía |
 * | `rechazados` con `id: null` | REJECTED_LOCAL, ubicado por su `indice` en el lote enviado |
 * | No mencionado | PENDING: se volverá a enviar |
 *
 * Los ids que no estaban en el lote enviado se ignoran.
 */
internal class InterpretarRespuestaLote {

    fun interpretar(ids: List<String>, respuesta: RespuestaLote): Map<String, AccionEvento> {
        val respondidos = idsRespondidos(respuesta)
        val sinId = rechazadosSinId(ids, respuesta)
        return ids.associateWith { id ->
            when (id) {
                in sinId -> AccionEvento.REJECTED_LOCAL
                in respondidos -> AccionEvento.SYNCED
                else -> AccionEvento.PENDING
            }
        }
    }

    private fun idsRespondidos(respuesta: RespuestaLote): Set<String> =
        (respuesta.aceptados + respuesta.duplicados + respuesta.rechazados.mapNotNull { it.id }).toSet()

    /** Un rechazo sin `id` (el evento no traía uno válido) solo puede ubicarse por su posición en el lote. */
    private fun rechazadosSinId(ids: List<String>, respuesta: RespuestaLote): Set<String> =
        respuesta.rechazados
            .filter { it.id == null }
            .mapNotNull { rechazo -> rechazo.indice?.let(ids::getOrNull) }
            .toSet()
}
