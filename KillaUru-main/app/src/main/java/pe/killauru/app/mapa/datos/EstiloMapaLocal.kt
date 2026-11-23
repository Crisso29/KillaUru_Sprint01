package pe.killauru.app.mapa.datos

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.jsonObject
import pe.killauru.app.mapa.dominio.FuenteEstiloMapa
import pe.killauru.app.mapa.dominio.TipoArchivoMapa
import java.io.File
import javax.inject.Inject

/**
 * FR-040, FR-044, T096: el estilo del mapa base para MapLibre, **sin red**. El estilo publicado apunta al PMTiles de la
 * CDN y a las letras de Protomaps (así lo usa el panel); aquí se cambian por el archivo descargado y por los glifos del
 * APK (`assets/fuentes`, Noto Sans, OFL).
 */
internal class EstiloMapaLocal @Inject constructor(private val archivos: ArchivosMapaLocal) : FuenteEstiloMapa {

    override suspend fun estilo(): String? = withContext(Dispatchers.IO) {
        val estilo = archivos.archivo(TipoArchivoMapa.ESTILO)?.takeIf { it.exists() } ?: return@withContext null
        val pmtiles = archivos.archivo(TipoArchivoMapa.PMTILES)?.takeIf { it.exists() } ?: return@withContext null
        adaptar(estilo.readText(), pmtiles)
    }

    companion object {
        const val GLIFOS_APK = "asset://fuentes/{fontstack}/{range}.pbf"
        private const val FUENTE = "protomaps"

        /** Mismo estilo, con la fuente `protomaps` leyendo [pmtiles] y las letras del APK. Función pura. */
        fun adaptar(estiloPublicado: String, pmtiles: File): String {
            val estilo = Json.parseToJsonElement(estiloPublicado).jsonObject
            val fuentes = estilo.getValue("sources").jsonObject
            val fuente = JsonObject(fuentes.getValue(FUENTE).jsonObject + ("url" to JsonPrimitive("pmtiles://file://${pmtiles.absolutePath}")))
            return JsonObject(estilo + mapOf("glyphs" to JsonPrimitive(GLIFOS_APK), "sources" to JsonObject(fuentes + (FUENTE to fuente))))
                .toString()
        }
    }
}
