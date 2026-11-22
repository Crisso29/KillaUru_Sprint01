package pe.killauru.app.mapa.datos

import kotlinx.coroutines.test.runTest
import kotlinx.serialization.json.Json
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.After
import org.junit.Before
import org.junit.Test
import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.mapa.dominio.Alcance
import pe.killauru.app.mapa.dominio.FuenteAnomalias
import pe.killauru.app.soporte.ValidadorContrato
import retrofit2.Retrofit
import retrofit2.converter.kotlinx.serialization.asConverterFactory
import java.time.Instant
import kotlin.test.assertEquals
import kotlin.test.assertIs
import kotlin.test.assertNull

/**
 * T085 / FR-044, FR-045, FR-078: el cliente del mapa sigue `specs/006-modulo-consulta-mapa/contracts/mapa.openapi.yaml`.
 * Los cuerpos de prueba se validan contra el esquema `ColeccionAnomalias` del YAML antes de usarlos. Patrón AAA.
 */
class ContratoMapaTest {
    private val yaml = "006-modulo-consulta-mapa/contracts/mapa.openapi.yaml"
    private val json = Json { ignoreUnknownKeys = true }
    private lateinit var servidor: MockWebServer
    private lateinit var fuente: RepositorioAnomaliasHttp

    private val alta = """
        {"type":"Feature","id":"0192f1b5-0a44-7d20-8c1e-5e3b9f2d6a70",
         "geometry":{"type":"Point","coordinates":[-74.2581,-13.2105]},
         "properties":{"id":"0192f1b5-0a44-7d20-8c1e-5e3b9f2d6a70","kmVia":287.4,"tipo":"Bache","severidad":4,
           "persistencia":"Persistente","ultimaDeteccionUtc":"2026-10-20T14:00:00Z","venceUtc":"2026-10-27T14:00:00Z"}}
    """.trimIndent()
    private val baja = """
        {"type":"Feature","id":"0192f1b5-0b10-7f00-9e2a-7c1d2e3f4a5b","geometry":null,
         "properties":{"id":"0192f1b5-0b10-7f00-9e2a-7c1d2e3f4a5b","eliminada":true}}
    """.trimIndent()

    private fun coleccion(vararg features: String) =
        """{"type":"FeatureCollection","generadoUtc":"2026-10-20T15:00:00Z","features":[${features.joinToString(",")}]}"""

    @Before fun setUp() {
        servidor = MockWebServer().apply { start() }
        val retrofit = Retrofit.Builder().baseUrl(servidor.url("/"))
            .addConverterFactory(json.asConverterFactory("application/json".toMediaType()))
            .build()
        fuente = RepositorioAnomaliasHttp(retrofit.create(MapaApi::class.java))
    }

    @After fun tearDown() = servidor.shutdown()

    private fun responder(respuesta: MockResponse) = servidor.enqueue(respuesta)

    @Test fun `los cuerpos de prueba cumplen el esquema ColeccionAnomalias del contrato`() {
        // Arrange
        val esquema = ValidadorContrato.esquema(yaml, "ColeccionAnomalias")
        // Act
        val errores = ValidadorContrato.validarJson(coleccion(alta, baja), esquema)
        // Assert
        assertEquals(emptyList(), errores)
    }

    @Test fun `anonimo pide GET a la ruta del contrato revalidando con If-None-Match`() = runTest {
        // Arrange
        responder(MockResponse().setResponseCode(304))
        // Act
        fuente.obtener(Alcance.Anonimo, updatedSince = null, etag = "\"v1\"")
        // Assert
        val peticion = servidor.takeRequest()
        assertEquals("GET", peticion.method)
        assertEquals("/api/v1/anomalies", peticion.requestUrl?.encodedPath)
        assertEquals("\"v1\"", peticion.getHeader("If-None-Match"))
        assertNull(peticion.requestUrl?.queryParameter("updatedSince"))
    }

    @Test fun `registrado con copia envia updatedSince como fecha ISO-8601`() = runTest {
        // Arrange
        responder(MockResponse().setBody(coleccion()))
        // Act
        fuente.obtener(Alcance.Registrado, updatedSince = 5_000L, etag = null)
        // Assert
        assertEquals("1970-01-01T00:00:05Z", servidor.takeRequest().requestUrl?.queryParameter("updatedSince"))
    }

    @Test fun `AnomaliaFeature del contrato se traduce a una alta del dominio`() = runTest {
        // Arrange
        responder(MockResponse().setBody(coleccion(alta)).addHeader("ETag", "\"v2\""))
        // Act
        val respuesta = fuente.obtener(Alcance.Anonimo, null, null)
        // Assert
        assertIs<FuenteAnomalias.Respuesta.Datos>(respuesta)
        val anomalia = respuesta.altas.single()
        assertEquals(Coordenada(latitud = -13.2105, longitud = -74.2581), anomalia.coordenada)
        assertEquals(287.4, anomalia.km)
        assertEquals(true, anomalia.persistente)
        assertEquals(Instant.parse("2026-10-27T14:00:00Z").toEpochMilli(), anomalia.venceUtc)
        assertEquals(Instant.parse("2026-10-20T15:00:00Z").toEpochMilli(), respuesta.generadoUtc)
        assertEquals("\"v2\"", respuesta.etag)
    }

    @Test fun `BajaFeature del contrato se traduce a una baja del dominio`() = runTest {
        // Arrange
        responder(MockResponse().setBody(coleccion(baja)))
        // Act
        val respuesta = fuente.obtener(Alcance.Registrado, updatedSince = 1_000L, etag = null)
        // Assert
        assertIs<FuenteAnomalias.Respuesta.Datos>(respuesta)
        assertEquals(listOf("0192f1b5-0b10-7f00-9e2a-7c1d2e3f4a5b"), respuesta.bajas)
    }

    @Test fun `304 toma la hora del servidor de la cabecera Date`() = runTest {
        // Arrange — formato HTTP (RFC 1123), no ISO-8601
        responder(MockResponse().setResponseCode(304).addHeader("Date", "Tue, 20 Oct 2026 15:00:00 GMT"))
        // Act
        val respuesta = fuente.obtener(Alcance.Anonimo, null, "\"v1\"")
        // Assert
        assertEquals(FuenteAnomalias.Respuesta.SinCambios(Instant.parse("2026-10-20T15:00:00Z").toEpochMilli()), respuesta)
    }

    @Test fun `304 sin cabecera Date no inventa una hora con el reloj del celular`() = runTest {
        // Arrange
        responder(MockResponse().setResponseCode(304).removeHeader("Date"))
        // Act
        val respuesta = fuente.obtener(Alcance.Anonimo, null, "\"v1\"")
        // Assert
        assertEquals(FuenteAnomalias.Respuesta.SinCambios(verificadoUtc = null), respuesta)
    }

    @Test fun `400 se traduce a SolicitudInvalida`() = runTest {
        // Arrange
        responder(MockResponse().setResponseCode(400))
        // Act
        val respuesta = fuente.obtener(Alcance.Registrado, updatedSince = 1_000L, etag = null)
        // Assert
        assertIs<FuenteAnomalias.Respuesta.SolicitudInvalida>(respuesta)
    }
}
