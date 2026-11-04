package pe.killauru.app.sincronizacion.dominio

import pe.killauru.app.comun.dominio.ParametrosApp
import pe.killauru.app.deteccion.dominio.EstadoLocal
import pe.killauru.app.deteccion.dominio.RepositorioEventos
import javax.inject.Inject

/** FR-021, FR-022, FR-029, FR-037: sincroniza eventos en lotes, expira viejos. */
internal class SincronizarEventosUseCase @Inject constructor(
    private val repositorio: RepositorioEventos,
    private val enviarLote: EnviarLoteUseCase,
    private val parametros: ParametrosApp,
) {
    suspend fun ejecutar(soloVerMapa: Boolean, enAhorro: Boolean) {
        if (soloVerMapa || enAhorro) return

        val limiteExpiracion = System.currentTimeMillis() - 168 * 60 * 60 * 1000L
        repositorio.expirar(limiteExpiracion)

        val pendientes = repositorio.tomarLote("", parametros.maxEventosPorLote * 10)
        if (pendientes.isEmpty()) return

        val porSeudonimo = pendientes.groupBy { it.seudonimo }
        for ((seudonimo, eventos) in porSeudonimo) {
            val lotes = PlanParticion(parametros.maxEventosPorLote).partir(eventos.map { it.eventoId })
            for (lote in lotes) {
                enviarLote.ejecutar(seudonimo, lote, eventos.filter { it.eventoId in lote })
            }
        }
    }
}
