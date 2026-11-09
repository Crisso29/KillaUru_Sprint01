package pe.killauru.app.deteccion.datos

import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map
import pe.killauru.app.deteccion.dominio.*
import java.time.Instant
import javax.inject.Inject
import javax.inject.Singleton

/** FR-019: implementación de RepositorioEventos sobre Room. */
@Singleton
internal class RepositorioEventosRoom @Inject constructor(
    private val dao: EventoDao,
    private val ahoraMs: () -> Long,
) : RepositorioEventos {

    override suspend fun guardar(evento: Evento) = dao.insertar(evento.aEntity())

    override fun pendientes(): Flow<List<Evento>> =
        dao.pendientesFlow().map { it.map { e -> e.aDominio() } }

    override suspend fun contarPendientes() = dao.contarPendientes()

    override suspend fun contarEnviando() = dao.contarEnviando()

    override suspend fun tomarLote(seudonimo: String, max: Int): List<Evento> =
        dao.tomarLote(seudonimo, max).map { it.aDominio() }

    override suspend fun expirar(anteriorAUtcMs: Long) =
        dao.expirar(anteriorAUtcMs, ahoraMs())

    override suspend fun recuperarEnviando() = dao.recuperarEnviando(ahoraMs())

    override suspend fun aplicarEstados(cambios: Map<String, EstadoLocal>) =
        dao.aplicarEstados(cambios.mapValues { it.value.codigo }, ahoraMs())

    private fun Evento.aEntity() = EventoEntity(
        eventoId = eventoId,
        detectadoUtc = detectadoUtc.toEpochMilli(),
        lat = lat, lon = lon, precisionM = precisionM,
        tipo = tipo.codigo, severidad = severidad,
        confidence = confidence, modeloVersion = modeloVersion,
        seudonimo = seudonimo, estado = estado.codigo,
        estadoDesde = estadoDesde.toEpochMilli(),
    )

    private fun EventoEntity.aDominio() = Evento(
        eventoId = eventoId,
        detectadoUtc = Instant.ofEpochMilli(detectadoUtc),
        lat = lat, lon = lon, precisionM = precisionM,
        tipo = TipoAnomalia.deCodigo(tipo),
        severidad = severidad, confidence = confidence,
        modeloVersion = modeloVersion, seudonimo = seudonimo,
        estado = EstadoLocal.deCodigo(estado),
        estadoDesde = Instant.ofEpochMilli(estadoDesde),
    )
}
