package pe.killauru.app.deteccion.dominio

import kotlinx.coroutines.flow.Flow

/** FR-019, FR-021: puerto para persistir y consultar eventos de anomalía. */
internal interface RepositorioEventos {
    suspend fun guardar(evento: Evento)
    fun pendientes(): Flow<List<Evento>>
    suspend fun contarPendientes(): Int
    suspend fun contarEnviando(): Int
    suspend fun tomarLote(seudonimo: String, max: Int): List<Evento>
    suspend fun expirar(anteriorAUtcMs: Long)
    suspend fun recuperarEnviando()
    suspend fun aplicarEstados(cambios: Map<String, EstadoLocal>)
}
