package pe.killauru.app.sincronizacion.dominio

import pe.killauru.app.comun.dominio.UuidV7
import pe.killauru.app.deteccion.dominio.EstadoLocal
import pe.killauru.app.deteccion.dominio.Evento
import pe.killauru.app.deteccion.dominio.RepositorioEventos
import javax.inject.Inject

/** FR-034, R-008: envía un lote y maneja 401/403 según el spec. */
internal class EnviarLoteUseCase @Inject constructor(
    private val api: ApiLotes,
    private val repositorio: RepositorioEventos,
) {
    suspend fun ejecutar(seudonimo: String, ids: List<String>, eventos: List<Evento>) {
        if (eventos.isEmpty()) return
        val lote = Lote(loteId = UuidV7.de(eventos.first().detectadoUtc, java.security.SecureRandom()).toString(), seudonimo = seudonimo, eventos = eventos)
        repositorio.aplicarEstados(ids.associateWith { EstadoLocal.ENVIANDO })

        val resp = api.enviar(lote)
        val estados = if (resp is RespuestaLoteApi.Exitosa) {
            InterpretarRespuestaLote().interpretar(ids, resp.dto).mapValues { estado(it.value) }
        } else ids.associateWith { EstadoLocal.PENDING }
        repositorio.aplicarEstados(estados)
    }

    private fun estado(accion: AccionEvento): EstadoLocal = when (accion) {
        AccionEvento.SYNCED -> EstadoLocal.SYNCED
        AccionEvento.REJECTED_LOCAL -> EstadoLocal.REJECTED_LOCAL
        AccionEvento.PENDING -> EstadoLocal.PENDING
    }
}
