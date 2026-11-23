package pe.killauru.app.mapa.datos

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import kotlinx.serialization.Serializable
import kotlinx.serialization.json.Json
import pe.killauru.app.mapa.dominio.ArchivoMapa
import pe.killauru.app.mapa.dominio.ArchivosMapa
import pe.killauru.app.mapa.dominio.ManifiestoMapa
import pe.killauru.app.mapa.dominio.TipoArchivoMapa
import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import java.io.File
import javax.inject.Inject
import javax.inject.Singleton

/** Lo que se guarda de la versión activa: su manifiesto (nombres, atribución) y el ETag para el próximo 304. */
@Serializable
internal data class EstadoMapaDto(val etag: String? = null, val manifiesto: ManifiestoMapaDto)

/**
 * FR-043, FR-044: implementación de [ArchivosMapa] en `filesDir/mapa`. Activar escribe el estado de forma atómica
 * (archivo temporal y renombrado) y solo después borra lo de versiones anteriores: un corte a medias no deja la app
 * sin mapa.
 */
@Singleton
internal class ArchivosMapaLocal @Inject constructor(
    private val almacen: AlmacenArchivos,
    private val descarga: DescargaReanudable,
    private val prefs: RepositorioPreferencias,
) : ArchivosMapa {

    override suspend fun instalado(): ManifiestoMapa? = leer()?.manifiesto?.let(MapeadorManifiesto::aDominio)

    override suspend fun etag(): String? = leer()?.etag

    override suspend fun descargar(archivo: ArchivoMapa, alProgresar: (bytesRecibidos: Long) -> Unit): Boolean =
        descarga.descargar(archivo.url, archivo.nombre, archivo.sha256, archivo.bytes, alProgresar) is
            DescargaReanudable.Resultado.Exitosa

    override suspend fun activar(manifiesto: ManifiestoMapa, etag: String?) = withContext(Dispatchers.IO) {
        val temporal = File(almacen.estado().path + ".tmp")
        temporal.writeText(JSON.encodeToString(EstadoMapaDto.serializer(), EstadoMapaDto(etag, MapeadorManifiesto.aDto(manifiesto))))
        check(temporal.renameTo(almacen.estado())) { "No se pudo guardar el estado del mapa" }
        prefs.guardarMapaVersion(manifiesto.version)
        val vigentes = manifiesto.archivos.map { it.nombre }.toSet()
        almacen.listarDatos().filter { it.name !in vigentes && !it.name.endsWith(".parcial") }.forEach { it.delete() }
    }

    /** El archivo de la versión activa, o null si aún no hay mapa. */
    suspend fun archivo(tipo: TipoArchivoMapa): File? = instalado()?.archivo(tipo)?.let { almacen.archivoMapa(it.nombre) }

    private suspend fun leer(): EstadoMapaDto? = withContext(Dispatchers.IO) {
        runCatching { JSON.decodeFromString(EstadoMapaDto.serializer(), almacen.estado().readText()) }.getOrNull()
    }

    private companion object {
        val JSON = Json { ignoreUnknownKeys = true }
    }
}
