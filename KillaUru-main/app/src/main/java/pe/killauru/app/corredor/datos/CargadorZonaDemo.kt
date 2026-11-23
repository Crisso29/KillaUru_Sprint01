package pe.killauru.app.corredor.datos

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import kotlinx.serialization.Serializable
import kotlinx.serialization.json.Json
import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.mapa.datos.ArchivosMapaLocal
import pe.killauru.app.mapa.dominio.FuenteZonaDemo
import pe.killauru.app.mapa.dominio.TipoArchivoMapa
import pe.killauru.app.mapa.dominio.ZonaDemo
import java.io.File
import javax.inject.Inject
import javax.inject.Singleton

/** GeoJSON `Feature` con un `Polygon` (`backend/db/datos/tramo_demo.geojson`, 006 T002). */
@Serializable
private data class TramoDemoDto(val geometry: GeometriaDto)

@Serializable
private data class GeometriaDto(val coordinates: List<List<List<Double>>>)

/**
 * FR-049, FR-051, RN-08: lee el tramo demo de la versión activa del mapa; es el mismo polígono que usa el servidor
 * para recortar al anónimo. Sin mapa descargado devuelve null.
 */
@Singleton
internal class CargadorZonaDemo @Inject constructor(private val archivos: ArchivosMapaLocal) : FuenteZonaDemo {
    private var enMemoria: Pair<File, ZonaDemo>? = null

    override suspend fun zona(): ZonaDemo? {
        val archivo = archivos.archivo(TipoArchivoMapa.TRAMO_DEMO)?.takeIf { it.exists() } ?: return null
        enMemoria?.takeIf { it.first == archivo }?.let { return it.second }
        return leer(archivo).also { enMemoria = archivo to it }
    }

    private suspend fun leer(archivo: File): ZonaDemo = withContext(Dispatchers.IO) {
        val anillo = JSON.decodeFromString(TramoDemoDto.serializer(), archivo.readText()).geometry.coordinates.first()
        ZonaDemo(anillo.map { (lon, lat) -> Coordenada(latitud = lat, longitud = lon) }) // GeoJSON: [lon, lat]
    }

    private companion object {
        val JSON = Json { ignoreUnknownKeys = true }
    }
}
