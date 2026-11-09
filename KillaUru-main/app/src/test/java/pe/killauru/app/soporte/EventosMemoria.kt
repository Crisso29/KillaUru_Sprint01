package pe.killauru.app.soporte

import kotlinx.coroutines.flow.flowOf
import pe.killauru.app.deteccion.dominio.*

internal class EventosMemoria : RepositorioEventos {
    val eventos = mutableListOf<Evento>()
    var expiracion: Long? = null
    var recuperaciones = 0
    override suspend fun guardar(evento: Evento) { eventos.add(evento) }
    override fun pendientes() = flowOf(eventos.toList())
    override suspend fun contarPendientes() = eventos.count { it.estado == EstadoLocal.PENDING }
    override suspend fun contarEnviando() = eventos.count { it.estado == EstadoLocal.ENVIANDO }
    override suspend fun tomarLote(seudonimo: String, max: Int) = eventos.filter { it.seudonimo == seudonimo }.take(max)
    override suspend fun expirar(anteriorAUtcMs: Long) { expiracion = anteriorAUtcMs }
    override suspend fun recuperarEnviando() { recuperaciones++ }
    override suspend fun aplicarEstados(cambios: Map<String, EstadoLocal>) = Unit
}
