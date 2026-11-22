package pe.killauru.app.mapa.datos

import kotlinx.coroutines.CancellationException
import pe.killauru.app.mapa.dominio.Alcance
import pe.killauru.app.mapa.dominio.FuenteAnomalias
import retrofit2.Response
import java.time.Instant
import javax.inject.Inject
import javax.inject.Singleton

/**
 * FR-044, FR-045: implementación HTTP de [FuenteAnomalias] con el contrato 006.
 * El servidor decide el alcance por el token (RN-08), por eso [Alcance] no viaja en la petición.
 * Nunca usa el reloj del celular (FR-026): la hora de un 304 es la cabecera `Date` del servidor.
 */
@Singleton
internal class RepositorioAnomaliasHttp @Inject constructor(
    private val api: MapaApi,
) : FuenteAnomalias {

    override suspend fun obtener(alcance: Alcance, updatedSince: Long?, etag: String?): FuenteAnomalias.Respuesta =
        try {
            traducir(api.anomalias(etag = etag, updatedSince = updatedSince?.let { Instant.ofEpochMilli(it).toString() }))
        } catch (e: CancellationException) {
            throw e // cancelar la corrutina no es un error de red: hay que propagarlo
        } catch (e: Exception) {
            FuenteAnomalias.Respuesta.Error(e)
        }

    private fun traducir(resp: Response<ColeccionAnomaliasDto>): FuenteAnomalias.Respuesta = when (resp.code()) {
        200 -> resp.body()?.let { datos(it, resp.headers()["ETag"]) }
            ?: FuenteAnomalias.Respuesta.Error(IllegalStateException("200 sin cuerpo"))
        304 -> FuenteAnomalias.Respuesta.SinCambios(verificadoUtc = resp.headers().getDate("Date")?.time)
        400 -> FuenteAnomalias.Respuesta.SolicitudInvalida
        else -> FuenteAnomalias.Respuesta.Error(IllegalStateException("HTTP ${resp.code()}"))
    }

    private fun datos(dto: ColeccionAnomaliasDto, etag: String?): FuenteAnomalias.Respuesta.Datos {
        val lote = MapeadorGeoJson.mapear(dto)
        return FuenteAnomalias.Respuesta.Datos(
            altas = lote.altas,
            bajas = lote.bajas,
            generadoUtc = MapeadorGeoJson.aMilisegundos(dto.generadoUtc),
            etag = etag,
        )
    }
}
