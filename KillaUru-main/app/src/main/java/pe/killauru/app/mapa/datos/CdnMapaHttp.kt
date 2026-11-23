package pe.killauru.app.mapa.datos

import kotlinx.coroutines.CancellationException
import pe.killauru.app.mapa.dominio.CdnMapa
import pe.killauru.app.mapa.dominio.RespuestaManifiesto
import javax.inject.Inject
import javax.inject.Named

/** FR-042: pide `mapa/manifiesto.json` a la CDN con el ETag guardado; 304 → sin cambios. */
internal class CdnMapaHttp @Inject constructor(
    private val api: CdnApi,
    @Named(CdnModulo.URL_BASE) private val base: String,
) : CdnMapa {

    override suspend fun manifiesto(etagPrevio: String?): RespuestaManifiesto = try {
        val respuesta = api.manifiesto("${base.trimEnd('/')}/$RUTA", etagPrevio)
        when {
            respuesta.code() == NO_MODIFICADO -> RespuestaManifiesto.SinCambios
            !respuesta.isSuccessful -> RespuestaManifiesto.Fallo(IllegalStateException("HTTP ${respuesta.code()}"))
            else -> RespuestaManifiesto.Nuevo(MapeadorManifiesto.aDominio(checkNotNull(respuesta.body())), respuesta.headers()["ETag"])
        }
    } catch (e: CancellationException) {
        throw e
    } catch (e: Exception) {
        RespuestaManifiesto.Fallo(e)
    }

    private companion object {
        const val RUTA = "mapa/manifiesto.json"
        const val NO_MODIFICADO = 304
    }
}
