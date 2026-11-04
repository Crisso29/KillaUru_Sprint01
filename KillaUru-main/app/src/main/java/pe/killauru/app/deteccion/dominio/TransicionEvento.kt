package pe.killauru.app.deteccion.dominio

import java.time.Instant

/** FR-020, data-model §3: aplica una transición de estado validando la máquina. */
internal object TransicionEvento {

    private val transicionesValidas = setOf(
        EstadoLocal.PENDING to EstadoLocal.ENVIANDO,
        EstadoLocal.ENVIANDO to EstadoLocal.SYNCED,
        EstadoLocal.ENVIANDO to EstadoLocal.PENDING,
        EstadoLocal.ENVIANDO to EstadoLocal.REJECTED_LOCAL,
        EstadoLocal.PENDING to EstadoLocal.EXPIRADO_LOCAL,
    )

    fun aplicar(evento: Evento, nuevo: EstadoLocal, ahora: Instant): Evento {
        require(EstadoLocal.PENDING to nuevo in transicionesValidas ||
            EstadoLocal.ENVIANDO to nuevo in transicionesValidas ||
            evento.estado to nuevo in transicionesValidas) {
            "Transición ${evento.estado}→$nuevo no permitida"
        }
        require(evento.estado to nuevo in transicionesValidas) {
            "Transición ${evento.estado}→$nuevo no permitida"
        }
        return evento.copy(estado = nuevo, estadoDesde = ahora)
    }

    fun esValida(desde: EstadoLocal, hacia: EstadoLocal): Boolean =
        desde to hacia in transicionesValidas
}
