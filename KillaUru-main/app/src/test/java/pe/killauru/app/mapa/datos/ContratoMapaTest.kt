package pe.killauru.app.mapa.datos

import com.jakewharton.retrofit2.converter.kotlinx.serialization.asConverterFactory
import kotlinx.coroutines.test.runTest
import kotlinx.serialization.json.Json
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.After
import org.junit.Assume.assumeTrue
import org.junit.Before
import org.junit.Test
import pe.killauru.app.mapa.dominio.Alcance
import pe.killauru.app.mapa.dominio.FuenteAnomalias
import pe.killauru.app.soporte.ValidadorContrato
import retrofit2.Retrofit
import kotlin.test.assertEquals
import kotlin.test.assertIs
import kotlin.test.assertNull
import kotlin.test.assertNotNull

/**
 * T085 / FR-044, FR-045, FR-078, SC-009: contrato HTTP del endpoint de anomalías.
 * Verifica que el cliente parse correctamente las respuestas del contrato OpenAPI.
 * MockWebServer simula el servidor; los fixtures reflejan el YAML real.
 * Patrón AAA en cada test.
 */
class ContratoMapaTest {

    private lateinit var server: MockWebServer
    private lateinit var fuente: RepositorioAnomaliasHttp

    private val json = Json { ignoreUnknownKeys = true; isLenient = true }

    // JSON mínimo válido de una AnomaliaFeature del contrato
    private val anomaliaFeatureJson = """
        {
          "type": "Feature",
          "id": "uuid-001",
          "geometry": {"type": "Point", "coordinates": [-74.5, -13.5]},
          "properties": {
            "tipo": "Bache",
            "severidad": 3,
            "persistente": false,
            "kmVia": 290.0,
            "ultimaDeteccionUtc": "2024-01-01T00:00:00Z",
            "venceUtc": "2099-12-31T23:59:59Z"
          }
        }
    """.trimIndent()

    // JSON mínimo válido de una BajaFeature (geometry null, eliminada true)
    private val bajaFeatureJson = """
        {
          "type": "Feature",
          "id": "uuid-del",
          "geometry": null,
          "properties": { "eliminada": true }
        }
    """.trimIndent()

    private fun coleccionJson(vararg features: String, generadoUtc: String = "2024-06-01T12:00:00Z") =
        """{"type":"FeatureCollection","generadoUtc":"$generadoUtc","features":[${features.joinToString(",")}]}"""

    @Before fun setUp() {
        server = MockWebServer()
        server.start()
        val retrofit = Retrofit.Builder()
            .baseUrl(server.url("/"))
            .client(OkHttpClient())
            .addConverterFactory(json.asConverterFactory("application/json".toMediaType()))
            .build()
        fuente = RepositorioAnomaliasHttp(retrofit.create(MapaApi::class.java))
    }

    @After fun tearDown() {
        server.shutdown()
    }

    // ── 200 AnomaliaFeature ──────────────────────────────────────────────────

    @Test fun `200 con AnomaliaFeature se mapea como alta`() = runTest {
        // Arrange
        server.enqueue(MockResponse().setBody(coleccionJson(anomaliaFeatureJson)).setResponseCode(200))
        // Act
        val respuesta = fuente.obtener(Alcance.Anonimo, null, null)
        // Assert
        assertIs<FuenteAnomalias.Respuesta.Datos>(respuesta)
        assertEquals(1, respuesta.altas.size)
        assertEquals("uuid-001", respuesta.altas.first().id)
        assertEquals(0, respuesta.bajas.size)
    }

    @Test fun `200 con BajaFeature se mapea como baja`() = runTest {
        // Arrange
        server.enqueue(MockResponse().setBody(coleccionJson(bajaFeatureJson)).setResponseCode(200))
        // Act
        val respuesta = fuente.obtener(Alcance.Registrado, updatedSince = 1000L, etag = null)
        // Assert
        assertIs<FuenteAnomalias.Respuesta.Datos>(respuesta)
        assertEquals(0, respuesta.altas.size)
        assertEquals(1, respuesta.bajas.size)
        assertEquals("uuid-del", respuesta.bajas.first())
    }

    @Test fun `200 retiene el ETag devuelto por el servidor`() = runTest {
        // Arrange
        server.enqueue(
            MockResponse()
                .setBody(coleccionJson(anomaliaFeatureJson))
                .setResponseCode(200)
                .addHeader("ETag", "\"abc123\"")
        )
        // Act
        val respuesta = fuente.obtener(Alcance.Anonimo, null, null)
        // Assert
        assertIs<FuenteAnomalias.Respuesta.Datos>(respuesta)
        assertEquals("\"abc123\"", respuesta.etag)
    }

    @Test fun `anonimo no envia updatedSince en la peticion`() = runTest {
        // Arrange
        server.enqueue(MockResponse().setBody(coleccionJson()).setResponseCode(200))
        // Act
        fuente.obtener(Alcance.Anonimo, updatedSince = null, etag = null)
        // Assert
        val peticion = server.takeRequest()
        assertNull(
            peticion.requestUrl?.queryParameter("desde"),
            "anónimo nunca debe enviar el parámetro 'desde'"
        )
    }

    @Test fun `registrado con updatedSince lo envia en la peticion`() = runTest {
        // Arrange
        server.enqueue(MockResponse().setBody(coleccionJson()).setResponseCode(200))
        // Act
        fuente.obtener(Alcance.Registrado, updatedSince = 5000L, etag = null)
        // Assert
        val peticion = server.takeRequest()
        assertNotNull(
            peticion.requestUrl?.queryParameter("desde"),
            "registrado con copia debe enviar 'desde'"
        )
        assertEquals("5000", peticion.requestUrl?.queryParameter("desde"))
    }

    // ── 304 ─────────────────────────────────────────────────────────────────

    @Test fun `304 se traduce a SinCambios`() = runTest {
        // Arrange
        server.enqueue(
            MockResponse()
                .setResponseCode(304)
                .addHeader("Date", "Mon, 01 Jan 2024 00:00:00 GMT")
        )
        // Act
        val respuesta = fuente.obtener(Alcance.Anonimo, null, "\"etag\"")
        // Assert
        assertIs<FuenteAnomalias.Respuesta.SinCambios>(respuesta)
    }

    // ── 400 ─────────────────────────────────────────────────────────────────

    @Test fun `400 se traduce a SolicitudInvalida`() = runTest {
        // Arrange
        server.enqueue(MockResponse().setResponseCode(400))
        // Act
        val respuesta = fuente.obtener(Alcance.Registrado, updatedSince = -1L, etag = null)
        // Assert
        assertIs<FuenteAnomalias.Respuesta.SolicitudInvalida>(respuesta)
    }

    // ── Validación del contrato YAML ─────────────────────────────────────────

    @Test fun `el YAML del contrato de mapa se puede parsear`() {
        // Arrange
        val yamlNombre = "006-modulo-consulta-mapa/contracts/mapa.openapi.yaml"
        assumeTrue(
            "YAML no copiado — ejecutar con la tarea copiarContratos del gradle",
            ValidadorContrato.estaPresente(yamlNombre)
        )
        // Act
        val resultado = runCatching { ValidadorContrato.cargarYaml(yamlNombre) }
        // Assert
        assertEquals(true, resultado.isSuccess, "El YAML debe ser parseable: ${resultado.exceptionOrNull()?.message}")
    }
}
