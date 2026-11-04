package pe.killauru.app.sincronizacion.dominio

/** FR-031, D5: mapea la respuesta del servidor a acciones por eventoId. */
internal class InterpretarRespuestaLote {
    fun interpretar(ids: List<String>, respuesta: RespuestaLoteDto): Map<String, AccionEvento> {
        val resultado = mutableMapOf<String, AccionEvento>()

        for (id in respuesta.aceptados) {
            if (id in ids) resultado[id] = AccionEvento.SYNCED
        }

        for (rechazo in respuesta.rechazados) {
            val id = rechazo.id
                ?: ids.getOrNull(rechazo.indice ?: -1)
                ?: continue
            if (id in ids) resultado[id] = AccionEvento.REJECTED_LOCAL
        }

        for (id in ids) {
            if (id !in resultado) resultado[id] = AccionEvento.PENDING
        }

        return resultado
    }
}
