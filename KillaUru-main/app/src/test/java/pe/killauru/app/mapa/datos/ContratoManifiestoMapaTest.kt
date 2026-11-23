package pe.killauru.app.mapa.datos

import io.swagger.v3.oas.models.PathItem
import kotlinx.coroutines.test.runTest
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.After
import org.junit.Before
import org.junit.Test
import pe.killauru.app.mapa.dominio.RespuestaManifiesto
import pe.killauru.app.mapa.dominio.TipoArchivoMapa
import pe.killauru.app.soporte.ValidadorContrato
import java.time.Instant
import kotlin.test.assertEquals
import kotlin.test.assertIs
import kotlin.test.assertNull

/**
 * T088 / FR-042, FR-043, FR-078: el manifiesto del mapa según `cdn.openapi.yaml`. La respuesta es el **ejemplo del
 * propio YAML** (antes era un JSON escrito a mano con otros campos). MockWebServer hace de CDN. Patrón AAA.
 */
class ContratoManifiestoMapaTest {
    private val yaml = "008-app-android/contracts/cdn.openapi.yaml"
    private val ejemplo = ValidadorContrato.ejemploRespuesta(yaml, "/mapa/manifiesto.json", PathItem.HttpMethod.GET, "200")
    private lateinit var servidor: MockWebServer
    private lateinit var cdn: CdnMapaHttp

    @Before fun setUp() {
        servidor = MockWebServer().apply { start() }
        cdn = CdnMapaHttp(CdnModulo.cdnApi(), servidor.url("/").toString())
    }

    @After fun tearDown() = servidor.shutdown()

    @Test fun `el ejemplo del contrato cumple el esquema ManifiestoMapa`() {
        // Arrange + Act
        val errores = ValidadorContrato.validarJson(ejemplo, ValidadorContrato.esquema(yaml, "ManifiestoMapa"))
        // Assert
        assertEquals(emptyList(), errores)
    }

    @Test fun `el manifiesto se pide a mapa manifiesto json y se traduce con sus 4 archivos`() = runTest {
        // Arrange
        servidor.enqueue(MockResponse().setBody(ejemplo).setHeader("ETag", "\"v3\""))
        // Act
        val r = assertIs<RespuestaManifiesto.Nuevo>(cdn.manifiesto(etagPrevio = null))
        // Assert
        assertEquals("/mapa/manifiesto.json", servidor.takeRequest().path)
        assertEquals("3", r.manifiesto.version)
        assertEquals(Instant.parse("2026-11-02T15:00:00Z").toEpochMilli(), r.manifiesto.publicadoUtcMs)
        assertEquals(TipoArchivoMapa.entries.toSet(), r.manifiesto.archivos.map { it.tipo }.toSet())
        assertEquals(39845120L, r.manifiesto.archivo(TipoArchivoMapa.PMTILES).bytes)
        assertEquals("corredor-pe28a.v3.pmtiles", r.manifiesto.archivo(TipoArchivoMapa.PMTILES).nombre)
        assertEquals("\"v3\"", r.etag)
    }

    @Test fun `con el ETag guardado envia If-None-Match y el 304 es SinCambios`() = runTest {
        // Arrange
        servidor.enqueue(MockResponse().setResponseCode(304))
        // Act
        val r = cdn.manifiesto(etagPrevio = "\"v3\"")
        // Assert — no se vuelve a bajar el manifiesto si no cambió (contrato: 304)
        assertEquals("\"v3\"", servidor.takeRequest().getHeader("If-None-Match"))
        assertEquals(RespuestaManifiesto.SinCambios, r)
    }

    @Test fun `sin red o con un manifiesto roto devuelve Fallo en vez de romper`() = runTest {
        // Arrange
        servidor.enqueue(MockResponse().setBody("""{"esquema":1}"""))
        // Act
        val roto = cdn.manifiesto(null)
        servidor.shutdown()
        val sinRed = cdn.manifiesto(null)
        // Assert
        assertIs<RespuestaManifiesto.Fallo>(roto)
        assertIs<RespuestaManifiesto.Fallo>(sinRed)
    }

    @Test fun `a la CDN publica no le llegan el token ni el seudonimo del conductor`() = runTest {
        // Arrange
        servidor.enqueue(MockResponse().setBody(ejemplo))
        // Act
        cdn.manifiesto(null)
        // Assert — el mismo CdnApi que provee Hilt: su propio cliente, sin los interceptores de la API
        val peticion = servidor.takeRequest()
        assertNull(peticion.getHeader("Authorization"))
        assertNull(peticion.getHeader("X-KillaUru-Seudonimo"))
    }
}
