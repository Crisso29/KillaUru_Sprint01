package pe.killauru.app.corredor.datos

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import kotlinx.serialization.Serializable
import kotlinx.serialization.json.Json
import pe.killauru.app.corredor.dominio.Corredor
import pe.killauru.app.corredor.dominio.FuenteCorredor
import pe.killauru.app.corredor.dominio.Vertice
import pe.killauru.app.mapa.datos.ArchivosMapaLocal
import pe.killauru.app.mapa.dominio.TipoArchivoMapa
import java.io.File
import javax.inject.Inject
import javax.inject.Singleton

/** Contrato `Corredor` de `cdn.openapi.yaml`: vértices `[longitud, latitud, kmAcumulado]`. */
@Serializable
private data class CorredorDto(val version: String, val vertices: List<List<Double>>)

/**
 * FR-055, FR-074: lee el corredor de la versión activa del mapa (6 689 vértices). Lo guarda en memoria mientras no
 * cambie el archivo: se usa en cada fix del GPS. Sin mapa descargado devuelve null (la app no se cae).
 */
@Singleton
internal class CargadorCorredor @Inject constructor(private val archivos: ArchivosMapaLocal) : FuenteCorredor {
    private var enMemoria: Pair<File, Corredor>? = null

    override suspend fun corredor(): Corredor? {
        val archivo = archivos.archivo(TipoArchivoMapa.CORREDOR)?.takeIf { it.exists() } ?: return null
        enMemoria?.takeIf { it.first == archivo }?.let { return it.second }
        return leer(archivo).also { enMemoria = archivo to it }
    }

    private suspend fun leer(archivo: File): Corredor = withContext(Dispatchers.IO) {
        val dto = JSON.decodeFromString(CorredorDto.serializer(), archivo.readText())
        Corredor(dto.vertices.map { (lon, lat, km) -> Vertice(lat = lat, lon = lon, km = km) })
    }

    private companion object {
        val JSON = Json { ignoreUnknownKeys = true }
    }
}
