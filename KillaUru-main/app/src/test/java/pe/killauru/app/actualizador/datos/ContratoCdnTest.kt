package pe.killauru.app.actualizador.datos

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
import pe.killauru.app.mapa.datos.CdnApi
import pe.killauru.app.soporte.ValidadorContrato
import retrofit2.Retrofit
import kotlin.test.assertEquals
import kotlin.test.assertNotNull
import kotlin.test.assertNull
import kotlin.test.assertTrue

/**
 * T088 / FR-042, SC-011: contrato HTTP del CDN para manifiesto y descarga reanudable.
 * MockWebServer simula el CDN. Patrón AAA en cada test.
 */
class ContratoCdnTest {

    private lateinit var server: MockWebServer
    private lateinit var api: CdnApi

    private val json = Json { ignoreUnknownKeys = true; isLenient = true }

    // Fixture del manifiesto según el YAML del CDN
    private val manifiestoJson = """
        {
          "version": "3",
          "archivo": "corredor-pe28a.v3.pmtiles",
          "sha256": "b1946ac92492d2347c6235b4d2611184b1946ac92492d2347c6235b4d2611184",
          "tamanoBytes": 39845120,
          "generadoUtc": 1730565600000
        }
    """.trimIndent()

    @Before fun setUp() {
        server = MockWebServer()
        server.start()
        val retrofit = Retrofit.Builder()
            .baseUrl(server.url("/"))
            .client(OkHttpClient())
            .addConverterFactory(json.asConverterFactory("application/json".toMediaType()))
            .build()
        api = retrofit.create(CdnApi::class.java)
    }

    @After fun tearDown() {
        server.shutdown()
    }

    // ── Manifiesto ───────────────────────────────────────────────────────────

    @Test fun `200 manifiesto se deserializa correctamente`() = runTest {
        // Arrange
        server.enqueue(
            MockResponse()
                .setResponseCode(200)
                .setHeader("Content-Type", "application/json")
                .setBody(manifiestoJson)
        )
        val url = server.url("/mapa/manifiesto.json").toString()
        // Act
        val respuesta = api.manifiesto(url)
        // Assert
        assertTrue(respuesta.isSuccessful)
        val cuerpo = respuesta.body()
        assertNotNull(cuerpo)
        assertEquals("3", cuerpo.version)
        assertEquals(39845120L, cuerpo.tamanoBytes)
    }

    @Test fun `peticion al manifiesto no lleva cabecera Range`() = runTest {
        // Arrange
        server.enqueue(MockResponse().setResponseCode(200).setBody(manifiestoJson))
        val url = server.url("/mapa/manifiesto.json").toString()
        // Act
        api.manifiesto(url)
        // Assert
        val peticion = server.takeRequest()
        assertNull(peticion.getHeader("Range"), "el manifiesto nunca debe pedirse con Range")
    }

    // ── Descarga reanudable ──────────────────────────────────────────────────

    @Test fun `206 con Content-Range confirma que el servidor admite reanudacion`() = runTest {
        // Arrange
        val inicio = 24_117_248L
        val total = 39_845_120L
        server.enqueue(
            MockResponse()
                .setResponseCode(206)
                .addHeader("Content-Range", "bytes $inicio-${total - 1}/$total")
                .setBody(okio.Buffer().write(byteArrayOf(0x01, 0x02)))
        )
        val url = server.url("/mapa/corredor-pe28a.v3.pmtiles").toString()
        // Act
        val respuesta = api.descargar(url, rango = "bytes=$inicio-")
        // Assert
        assertEquals(206, respuesta.code())
        assertNotNull(respuesta.headers()["Content-Range"])
        assertTrue(respuesta.headers()["Content-Range"]!!.startsWith("bytes $inicio-"))
    }

    @Test fun `peticion con Range se envía con el encabezado correcto`() = runTest {
        // Arrange
        server.enqueue(
            MockResponse()
                .setResponseCode(206)
                .setBody(okio.Buffer().write(byteArrayOf()))
        )
        val rango = "bytes=1024-"
        val url = server.url("/mapa/corredor-pe28a.v3.pmtiles").toString()
        // Act
        api.descargar(url, rango = rango)
        // Assert
        val peticion = server.takeRequest()
        assertEquals(rango, peticion.getHeader("Range"))
    }

    @Test fun `peticion sin Range descarga desde el inicio`() = runTest {
        // Arrange
        server.enqueue(MockResponse().setResponseCode(200).setBody(okio.Buffer().write(byteArrayOf(0x00))))
        val url = server.url("/mapa/corredor-pe28a.v3.pmtiles").toString()
        // Act
        api.descargar(url, rango = null)
        // Assert
        val peticion = server.takeRequest()
        assertNull(peticion.getHeader("Range"), "sin rango no debe enviarse la cabecera Range")
    }

    // ── YAML del contrato ────────────────────────────────────────────────────

    @Test fun `el YAML del contrato CDN se puede parsear`() {
        // Arrange
        val yamlNombre = "008-app-android/contracts/cdn.openapi.yaml"
        assumeTrue(
            "YAML no copiado — ejecutar con la tarea copiarContratos del gradle",
            ValidadorContrato.estaPresente(yamlNombre)
        )
        // Act
        val resultado = runCatching { ValidadorContrato.cargarYaml(yamlNombre) }
        // Assert
        assertEquals(true, resultado.isSuccess, "El YAML del CDN debe ser parseable: ${resultado.exceptionOrNull()?.message}")
    }
}
