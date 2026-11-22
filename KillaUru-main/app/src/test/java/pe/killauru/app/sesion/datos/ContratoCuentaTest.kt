package pe.killauru.app.sesion.datos

import kotlinx.coroutines.test.runTest
import kotlinx.serialization.json.Json
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.After
import org.junit.Before
import org.junit.Test
import pe.killauru.app.sesion.dominio.DocumentoLegal
import pe.killauru.app.sesion.dominio.ResultadoRecuperacion
import pe.killauru.app.sesion.dominio.Rol
import pe.killauru.app.soporte.ValidadorContrato
import retrofit2.Retrofit
import retrofit2.converter.kotlinx.serialization.asConverterFactory
import java.time.Instant
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertNull
import kotlin.test.assertTrue

/**
 * FR-068 (ver mis datos) y HU-25 (recuperar la contraseña): el cliente sigue el contrato 003. MockWebServer hace de API;
 * el cuerpo enviado y el ejemplo de respuesta se validan contra el YAML. Patrón AAA.
 */
class ContratoCuentaTest {
    private val yaml = "003-modulo-identidad-accesos/contracts/identidad.openapi.yaml"
    private val json = Json { ignoreUnknownKeys = true }
    private lateinit var servidor: MockWebServer
    private lateinit var cuenta: ApiCuentaHttp

    private val misDatos = """
        {"correo":"conductor@ejemplo.pe","rol":"ConductorRegistrado","fechaAltaUtc":"2026-10-14T15:00:00Z",
         "correoVerificado":true,"aceptaciones":[
           {"documento":"PoliticaPrivacidad","version":"1.2","aceptadoUtc":"2026-10-14T15:00:00Z"},
           {"documento":"TerminosUso","version":"1.2","aceptadoUtc":"2026-10-14T15:00:00Z"}]}
    """.trimIndent()

    @Before fun setUp() {
        servidor = MockWebServer().apply { start() }
        val retrofit = Retrofit.Builder().baseUrl(servidor.url("/"))
            .addConverterFactory(json.asConverterFactory("application/json".toMediaType())).build()
        cuenta = ApiCuentaHttp(retrofit.create(IdentidadApi::class.java))
    }

    @After fun tearDown() = servidor.shutdown()

    @Test fun `el ejemplo de mis datos cumple el esquema MisDatos`() {
        // Arrange + Act
        val errores = ValidadorContrato.validarJson(misDatos, ValidadorContrato.esquema(yaml, "MisDatos"))
        // Assert
        assertEquals(emptyList(), errores)
    }

    @Test fun `mis datos se piden a GET me y se traducen al dominio`() = runTest {
        // Arrange
        servidor.enqueue(MockResponse().setBody(misDatos))
        // Act
        val datos = cuenta.misDatos()!!
        // Assert
        val peticion = servidor.takeRequest()
        assertEquals("GET", peticion.method)
        assertEquals("/api/v1/me", peticion.path)
        assertEquals("conductor@ejemplo.pe", datos.correo)
        assertEquals(Rol.ConductorRegistrado, datos.rol)
        assertEquals(Instant.parse("2026-10-14T15:00:00Z").toEpochMilli(), datos.fechaAltaUtcMs)
        assertEquals("1.2", datos.aceptacion(DocumentoLegal.TerminosUso)?.version)
    }

    @Test fun `sin red mis datos devuelve null en vez de romper`() = runTest {
        // Arrange
        servidor.shutdown()
        // Act + Assert
        assertNull(cuenta.misDatos())
    }

    @Test fun `recuperar envia el correo a la ruta del contrato y el 202 es Enviado`() = runTest {
        // Arrange
        servidor.enqueue(MockResponse().setResponseCode(202))
        // Act
        val r = cuenta.recuperarContrasena("conductor@ejemplo.pe")
        // Assert
        val peticion = servidor.takeRequest()
        assertEquals("/api/v1/auth/recuperar", peticion.path)
        assertEquals("""{"correo":"conductor@ejemplo.pe"}""", peticion.body.readUtf8())
        assertEquals(ResultadoRecuperacion.Enviado, r)
    }

    @Test fun `recuperar traduce 400 y 429`() = runTest {
        // Arrange
        servidor.enqueue(MockResponse().setResponseCode(400))
        servidor.enqueue(MockResponse().setResponseCode(429))
        // Act + Assert
        assertEquals(ResultadoRecuperacion.CorreoInvalido, cuenta.recuperarContrasena("x"))
        assertEquals(ResultadoRecuperacion.LimiteExcedido, cuenta.recuperarContrasena("conductor@ejemplo.pe"))
    }

    @Test fun `revocar la sesion envia el token de acceso y el de renovacion segun el contrato`() = runTest {
        // Arrange
        servidor.enqueue(MockResponse().setResponseCode(204))
        // Act
        val revocada = cuenta.revocarSesion("jwt-acceso", "opaco-renovacion")
        // Assert
        val peticion = servidor.takeRequest()
        assertEquals("/api/v1/auth/cerrar-sesion", peticion.path)
        assertEquals("Bearer jwt-acceso", peticion.getHeader("Authorization"))
        val cuerpo = peticion.body.readUtf8()
        assertEquals(emptyList(), ValidadorContrato.validarJson(cuerpo, ValidadorContrato.esquema(yaml, "RenovarSolicitud")))
        assertTrue(revocada)
    }

    /** 401: el servidor ya no reconoce esa sesión, no queda nada que revocar. 5xx o sin red: hay que reintentar. */
    @Test fun `revocar con 401 no reintenta y con 503 o sin red si`() = runTest {
        // Arrange
        servidor.enqueue(MockResponse().setResponseCode(401))
        servidor.enqueue(MockResponse().setResponseCode(503))
        // Act + Assert
        assertTrue(cuenta.revocarSesion("a", "r"))
        assertFalse(cuenta.revocarSesion("a", "r"))
        servidor.shutdown()
        assertFalse(cuenta.revocarSesion("a", "r"))
    }
}
