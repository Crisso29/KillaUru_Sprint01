package pe.killauru.app.sincronizacion.dominio

import pe.killauru.app.sincronizacion.dominio.PurgadorEventos
import javax.inject.Inject

/** FR-038: purga eventos SYNCED/REJECTED_LOCAL/EXPIRADO_LOCAL mayores a 7 días. */
internal class PurgarLocalUseCase @Inject constructor(private val purgador: PurgadorEventos) {
    suspend fun ejecutar(anteriorAUtcMs: Long) { purgador.purgarEventosFinales(anteriorAUtcMs) }
}
