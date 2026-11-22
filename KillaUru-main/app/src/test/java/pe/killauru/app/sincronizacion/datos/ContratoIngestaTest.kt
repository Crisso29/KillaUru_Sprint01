package pe.killauru.app.sincronizacion.datos

import io.swagger.v3.oas.models.PathItem
import kotlinx.coroutines.test.runTest
import kotlinx.serialization.json.Json
import kotlinx.serialization.json.jsonArray
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.After
import org.junit.Before
import org.junit.Test
import pe.killauru.app.deteccion.dominio.EstadoLocal
import pe.killauru.app.deteccion.dominio.Evento
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import pe.killauru.app.sincronizacion.dominio.Lote
import pe.killauru.app.sincronizacion.dominio.Rechazo
import pe.killauru.app.sincronizacion.dominio.RespuestaLoteApi
import pe.killauru.app.soporte.ValidadorContrato
import retrofit2.Retrofit
import retrofit2.converter.kotlinx.serialization.asConverterFactory
import java.time.Instant
import kotlin.test.assertEquals
import kotlin.test.assertIs
import kotlin.test.assertTrue

/**
 * FR-078, contrato `specs/005-modulo-ingesta/contracts/ingesta.openapi.yaml`: lo que la app envía y entiende
 * coincide con el YAML original (sin copias a mano). MockWebServer hace de API. Patrón AAA en cada prueba.
 */
class ContratoIngestaTest {
    private val yaml = "005-modulo-ingesta/contracts/ingesta.openapi.yaml"
    private val ruta = "/api/v1/anomalies/batch"
    private lateinit var servidor: MockWebServer
    private lateinit var cliente: ClienteLotesHttp
    private val json = Json { ignoreUnknownKeys = true }

    private val evento = Evento(
        eventoId = "0192f1b5-0a44-7d20-8c1e-5e3b9f2d6a70", detectadoUtc = Instant.parse("2026-10-20T14:32:05Z"),
        lat = -13.2105, lon = -74.2581, precisionM = 6.5, tipo = TipoAnomalia.Bache, severidad = 4,
        confidence = 0.91, modeloVersion = "1.2.0", seudonimo = "6b1c9d3e-2f4a-4b8e-9c7d-1a2b3c4d5e6f",
        estado = EstadoLocal.ENVIANDO, estadoDesde = Instant.EPOCH,
    )
    private val lote = Lote(loteId = "local", seudonimo = evento.seudonimo, eventos = listOf(evento))

    @Before fun setUp() {
        servidor = MockWebServer().apply { start() }
        val retrofit = Retrofit.Builder().baseUrl(servidor.url("/"))
            .addConverterFactory(json.asConverterFactory("application/json".toMediaType()))
            .build()
        cliente = ClienteLotesHttp(retrofit.create(IngestaApi::class.java))
    }

    @After fun tearDown() = servidor.shutdown()

    private fun responder(codigo: Int, cuerpo: String = "", vararg cabeceras: Pair<String, String>) =
        servidor.enqueue(MockResponse().setResponseCode(codigo).setBody(cuerpo).apply {
            cabeceras.forEach { (k, v) -> addHeader(k, v) }
        })

    @Test fun `envia POST a la ruta del contrato con el seudonimo del lote en la cabecera`() = runTest {
        // Arrange
        responder(200, """{"loteId":"x","aceptados":[],"duplicados":[],"rechazados":[]}""")
        // Act
        cliente.enviar(lote)
        // Assert
        val peticion = servidor.takeRequest()
        assertEquals("POST", peticion.method)
        assertEquals(ruta, peticion.requestUrl?.encodedPath)
        assertEquals(lote.seudonimo, peticion.getHeader("X-KillaUru-Seudonimo"))
    }

    @Test fun `el cuerpo valida contra Lote y cada evento solo usa campos del contrato`() = runTest {
        // Arrange
        responder(200, """{"loteId":"x","aceptados":[],"duplicados":[],"rechazados":[]}""")
        // Act
        cliente.enviar(lote)
        // Assert — el esquema Evento es permisivo (D5): por eso también se comparan los nombres de campo
        val cuerpo = ValidadorContrato.bodyDe(servidor.takeRequest())
        assertEquals(emptyList(), ValidadorContrato.validarJson(cuerpo, ValidadorContrato.esquema(yaml, "Lote")))
        val enviado = Json.parseToJsonElement(cuerpo).jsonObject["eventos"]!!.jsonArray.first().jsonObject
        assertTrue(ValidadorContrato.propiedades(yaml, "Evento").containsAll(enviado.keys), "campos fuera del contrato: ${enviado.keys}")
        assertEquals("Bache", enviado["tipo"]!!.jsonPrimitive.content)
        assertEquals(evento.seudonimo, enviado["seudonimo"]!!.jsonPrimitive.content)
    }

    @Test fun `el ejemplo 200 del contrato se traduce al dominio con duplicados y motivos`() = runTest {
        // Arrange — cuerpo tomado del `example` del YAML
        responder(200, ValidadorContrato.ejemploRespuesta(yaml, ruta, PathItem.HttpMethod.POST, "200"))
        // Act
        val respuesta = cliente.enviar(lote)
        // Assert
        assertIs<RespuestaLoteApi.Exitosa>(respuesta)
        assertEquals(listOf("0192f1b5-0a44-7d20-8c1e-5e3b9f2d6a70"), respuesta.respuesta.aceptados)
        assertEquals(emptyList(), respuesta.respuesta.duplicados)
        assertEquals(Rechazo(null, 2, "EsquemaInvalido"), respuesta.respuesta.rechazados[1])
    }

    @Test fun `400 lleva el codigo del ProblemDetails`() = runTest {
        // Arrange
        responder(400, """{"type":"t","title":"x","status":400,"codigo":"lote_invalido","correlacionId":"c"}""")
        // Act
        val respuesta = cliente.enviar(lote)
        // Assert
        assertEquals(RespuestaLoteApi.Error400("lote_invalido"), respuesta)
    }

    @Test fun `413 lleva el codigo del ProblemDetails`() = runTest {
        // Arrange
        responder(413, """{"type":"t","title":"x","status":413,"codigo":"lote_demasiado_grande","correlacionId":"c"}""")
        // Act
        val respuesta = cliente.enviar(lote)
        // Assert
        assertEquals(RespuestaLoteApi.Error413("lote_demasiado_grande"), respuesta)
    }

    @Test fun `429 y 503 entregan el Retry-After del servidor en milisegundos`() = runTest {
        // Arrange — FR-035: se reintenta tras Retry-After (segundos en la cabecera)
        responder(429, "{}", "Retry-After" to "120")
        responder(503, "{}", "Retry-After" to "30")
        // Act
        val limite = cliente.enviar(lote)
        val saturado = cliente.enviar(lote)
        // Assert
        assertEquals(RespuestaLoteApi.Error429(120_000L), limite)
        assertEquals(RespuestaLoteApi.Error503(30_000L), saturado)
    }
}
