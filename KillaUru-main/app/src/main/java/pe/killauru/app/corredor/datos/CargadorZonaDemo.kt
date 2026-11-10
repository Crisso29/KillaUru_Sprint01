package pe.killauru.app.corredor.datos

import android.content.Context
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable
import kotlinx.serialization.json.Json
import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.mapa.dominio.ZonaDemo
import javax.inject.Inject
import javax.inject.Singleton

/**
 * FR-049, FR-051: carga el polígono del tramo demo (máx 60 vértices) desde los assets.
 *
 * El archivo GeoJSON `mapa/tramo_demo.geojson` es generado por `006 T002` y distribuido
 * en el APK. Es estático; no se descarga.
 */
@Singleton
internal class CargadorZonaDemo @Inject constructor(
    @ApplicationContext private val ctx: Context,
    private val json: Json,
) {
    companion object {
        private const val ASSET_DEMO = "mapa/tramo_demo.geojson"
    }

    fun cargar(): ZonaDemo {
        val contenido = ctx.assets.open(ASSET_DEMO).bufferedReader().readText()
        val dto = json.decodeFromString<ZonaDemoDto>(contenido)
        val vertices = dto.geometry.coordinates.flatten().map { par ->
            // GeoJSON: [longitud, latitud]
            Coordenada(latitud = par[1], longitud = par[0])
        }
        return ZonaDemo(vertices)
    }
}

@Serializable
private data class ZonaDemoDto(
    @SerialName("geometry") val geometry: ZonaDemoGeometryDto,
)

@Serializable
private data class ZonaDemoGeometryDto(
    @SerialName("coordinates") val coordinates: List<List<List<Double>>>,
)
