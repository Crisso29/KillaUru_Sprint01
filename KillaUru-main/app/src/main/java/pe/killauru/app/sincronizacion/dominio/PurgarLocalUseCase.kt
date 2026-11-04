package pe.killauru.app.sincronizacion.dominio

import pe.killauru.app.comun.datos.bd.PurgaDao
import javax.inject.Inject

/** FR-038: purga eventos SYNCED/REJECTED_LOCAL/EXPIRADO_LOCAL mayores a 7 días. */
internal class PurgarLocalUseCase @Inject constructor(private val dao: PurgaDao) {
    suspend fun ejecutar(anteriorAUtcMs: Long) { dao.purgarEventosFinales(anteriorAUtcMs) }
}
