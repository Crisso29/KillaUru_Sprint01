package pe.killauru.app.sincronizacion.dominio

import pe.killauru.app.comun.dominio.UuidV7
import pe.killauru.app.deteccion.dominio.EstadoLocal
import pe.killauru.app.deteccion.dominio.Evento
import pe.killauru.app.deteccion.dominio.RepositorioEventos
import java.security.SecureRandom
import javax.inject.Inject

/**
 * FR-031, FR-034, R-008: envía un lote y aplica a cada evento el estado que corresponde a la respuesta.
 * Antes de enviar los marca ENVIANDO; tras un 200 decide [InterpretarRespuestaLote] y se cuentan los rechazos;
 * ante cualquier otra respuesta (401, 403, red…) vuelven a PENDING para reintentarse.
 */
internal class EnviarLoteUseCase @Inject constructor(
    private val api: ApiLotes,
    private val repositorio: RepositorioEventos,
    private val contador: ContadorRechazos,
) {
    private val aleatorio = SecureRandom()

    suspend fun ejecutar(seudonimo: String, ids: List<String>, eventos: List<Evento>) {
        if (eventos.isEmpty()) return
        repositorio.aplicarEstados(ids.associateWith { EstadoLocal.ENVIANDO })
        val resp = api.enviar(armarLote(seudonimo, eventos))
        repositorio.aplicarEstados(estadosTrasRespuesta(ids, resp))
        if (resp is RespuestaLoteApi.Exitosa) contador.sumar(resp.respuesta.rechazados.map { it.motivo })
    }

    /** El `loteId` local solo identifica el lote en la app; el servidor genera el suyo (D3). */
    private fun armarLote(seudonimo: String, eventos: List<Evento>) = Lote(
        loteId = UuidV7.de(eventos.first().detectadoUtc, aleatorio).toString(),
        seudonimo = seudonimo,
        eventos = eventos,
    )

    private fun estadosTrasRespuesta(ids: List<String>, resp: RespuestaLoteApi): Map<String, EstadoLocal> =
        if (resp is RespuestaLoteApi.Exitosa) {
            InterpretarRespuestaLote().interpretar(ids, resp.respuesta).mapValues { estado(it.value) }
        } else {
            ids.associateWith { EstadoLocal.PENDING }
        }

    private fun estado(accion: AccionEvento): EstadoLocal = when (accion) {
        AccionEvento.SYNCED -> EstadoLocal.SYNCED
        AccionEvento.REJECTED_LOCAL -> EstadoLocal.REJECTED_LOCAL
        AccionEvento.PENDING -> EstadoLocal.PENDING
    }
}
