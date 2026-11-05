package pe.killauru.app.sincronizacion.datos

import pe.killauru.app.comun.datos.bd.PurgaDao
import pe.killauru.app.sincronizacion.dominio.PurgadorEventos
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
internal class PurgadorEventosRoom @Inject constructor(
    private val dao: PurgaDao,
) : PurgadorEventos {
    override suspend fun purgarEventosFinales(anteriorAUtcMs: Long) {
        dao.purgarEventosFinales(anteriorAUtcMs)
    }
}
