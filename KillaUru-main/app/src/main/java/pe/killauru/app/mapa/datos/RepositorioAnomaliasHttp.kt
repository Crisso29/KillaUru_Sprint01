package pe.killauru.app.mapa.datos

import pe.killauru.app.mapa.dominio.Alcance
import pe.killauru.app.mapa.dominio.FuenteAnomalias
import javax.inject.Inject
import javax.inject.Singleton

/**
 * FR-044, FR-045: implementación HTTP del puerto [FuenteAnomalias].
 * Mapea las respuestas del servidor a los tipos sellados del dominio.
 */
@Singleton
internal class RepositorioAnomaliasHttp @Inject constructor(
    private val api: MapaApi,
) : FuenteAnomalias {

    override suspend fun obtener(
        alcance: Alcance,
        updatedSince: Long?,
        etag: String?,
    ): FuenteAnomalias.Respuesta = try {
        val resp = api.anomalias(
            etag = etag,
            desdeUtc = updatedSince,
        )
        when (resp.code()) {
            200 -> {
                val body = resp.body() ?: return FuenteAnomalias.Respuesta.SolicitudInvalida
                val lote = MapeadorGeoJson.mapear(body)
                FuenteAnomalias.Respuesta.Datos(
                    altas = lote.altas,
                    bajas = lote.bajas,
                    generadoUtc = body.generadoUtc,
                    etag = resp.headers()["ETag"],
                )
            }
            304 -> {
                // El servidor usa la hora del cliente como aproximación
                val verificado = resp.headers()["Date"]
                    ?.let { java.time.Instant.parse(it).toEpochMilli() }
                    ?: System.currentTimeMillis()
                FuenteAnomalias.Respuesta.SinCambios(verificado)
            }
            400 -> FuenteAnomalias.Respuesta.SolicitudInvalida
            else -> FuenteAnomalias.Respuesta.Error(
                RuntimeException("HTTP ${resp.code()}")
            )
        }
    } catch (e: Exception) {
        FuenteAnomalias.Respuesta.Error(e)
    }
}
