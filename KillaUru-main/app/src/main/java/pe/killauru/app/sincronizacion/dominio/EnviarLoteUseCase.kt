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
        val lote = Lote(loteId = UuidV7.generar(), seudonimo = seudonimo, eventos = eventos)
        repositorio.aplicarEstados(ids.associateWith { EstadoLocal.ENVIANDO })

        when (val resp = api.enviar(lote)) {
            is RespuestaLoteApi.Exitosa -> {
                val acciones = InterpretarRespuestaLote().interpretar(ids, resp.dto)
                repositorio.aplicarEstados(acciones.mapValues {
                    when (it.value) {
                        AccionEvento.SYNCED -> EstadoLocal.SYNCED
                        AccionEvento.REJECTED_LOCAL -> EstadoLocal.REJECTED_LOCAL
                        AccionEvento.PENDING -> EstadoLocal.PENDING
                    }
                })
            }
            is RespuestaLoteApi.Error401, is RespuestaLoteApi.Error403 -> {
                repositorio.aplicarEstados(ids.associateWith { EstadoLocal.PENDING })
            }
            is RespuestaLoteApi.ErrorRed -> {
                repositorio.aplicarEstados(ids.associateWith { EstadoLocal.PENDING })
            }
            else -> {
                repositorio.aplicarEstados(ids.associateWith { EstadoLocal.PENDING })
            }
        }
    }
}
