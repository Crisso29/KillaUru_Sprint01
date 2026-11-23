package pe.killauru.app.mapa.datos

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.jsonArray
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import org.junit.Test
import java.io.File
import kotlin.test.assertEquals

/**
 * FR-040, FR-044, T096: sin señal el mapa sale del celular. El estilo publicado (herramientas/cdn/generar_estilo.py)
 * apunta a la CDN; la app lo cambia por el PMTiles descargado y por las letras del APK. Patrón AAA.
 */
class EstiloMapaLocalTest {
    private val publicado = """{"version":8,"name":"KillaUru v1",
        "glyphs":"https://protomaps.github.io/basemaps-assets/fonts/{fontstack}/{range}.pbf",
        "sources":{"protomaps":{"type":"vector","url":"pmtiles://https://cdn.killauru.pe/mapa/corredor-pe28a.v1.pmtiles",
        "attribution":"© Colaboradores de OpenStreetMap"}},
        "layers":[{"id":"fondo","type":"background","paint":{"background-color":"#EEF1E7"}}]}"""

    private fun leer(json: String): JsonObject = Json.parseToJsonElement(json).jsonObject

    @Test fun `el mapa base se lee del pmtiles descargado y las letras del APK`() {
        // Arrange
        val pmtiles = File("/data/user/0/pe.killauru.app/files/mapa/corredor-pe28a.v1.pmtiles")
        // Act
        val local = leer(EstiloMapaLocal.adaptar(publicado, pmtiles))
        // Assert
        val fuente = local["sources"]!!.jsonObject["protomaps"]!!.jsonObject
        assertEquals("pmtiles://file://${pmtiles.absolutePath}", fuente["url"]!!.jsonPrimitive.content)
        assertEquals("asset://fuentes/{fontstack}/{range}.pbf", local["glyphs"]!!.jsonPrimitive.content)
    }

    @Test fun `conserva la atribucion y las capas tal cual`() {
        // Arrange + Act
        val local = leer(EstiloMapaLocal.adaptar(publicado, File("/x.pmtiles")))
        // Assert — ADR-011: la atribución de OpenStreetMap se muestra siempre
        assertEquals("© Colaboradores de OpenStreetMap",
            local["sources"]!!.jsonObject["protomaps"]!!.jsonObject["attribution"]!!.jsonPrimitive.content)
        assertEquals(leer(publicado)["layers"]!!.jsonArray, local["layers"]!!.jsonArray)
    }
}
