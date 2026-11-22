package pe.killauru.app.sesion.datos

import kotlinx.coroutines.test.runTest
import kotlinx.serialization.json.Json
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.After
import org.junit.Before
import org.junit.Test
import retrofit2.Retrofit
import retrofit2.converter.kotlinx.serialization.asConverterFactory
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertNull
import kotlin.test.assertTrue

/**
 * T107 / FR-066, R-023: la revocación que quedó pendiente por falta de red. Como el token de acceso ya venció (dura
 * 5 min), primero renueva y luego cierra la sesión con los tokens nuevos. Patrón AAA; MockWebServer hace de API.
 */
class EnviarRevocacionPendienteTest {
    private val json = Json { ignoreUnknownKeys = true }
    private lateinit var servidor: MockWebServer
    private lateinit var almacen: AlmacenMemoria
    private lateinit var envio: EnviarRevocacionPendiente

    private class AlmacenMemoria(var token: String? = null) : AlmacenRevocacion {
        override suspend fun guardar(tokenRenovacion: String) { token = tokenRenovacion }
        override suspend fun obtener() = token
        override suspend fun borrar() { token = null }
    }

    private val sesionNueva = """{"tokenAcceso":"acceso-nuevo","expiraAccesoUtc":"2026-11-22T10:05:00Z",
        "tokenRenovacion":"renovacion-nueva","sesionVenceUtc":"2026-12-22T10:00:00Z","rol":"ConductorRegistrado"}"""

    @Before fun setUp() {
        servidor = MockWebServer().apply { start() }
        val api = Retrofit.Builder().baseUrl(servidor.url("/"))
            .addConverterFactory(json.asConverterFactory("application/json".toMediaType()))
            .build().create(IdentidadApi::class.java)
        almacen = AlmacenMemoria("renovacion-vieja")
        envio = EnviarRevocacionPendiente(api, almacen)
    }

    @After fun tearDown() = servidor.shutdown()

    @Test fun `renueva y cierra la sesion con los tokens nuevos y olvida el pendiente`() = runTest {
        // Arrange
        servidor.enqueue(MockResponse().setBody(sesionNueva))
        servidor.enqueue(MockResponse().setResponseCode(204))
        // Act
        val terminado = envio.ejecutar()
        // Assert
        assertEquals("/api/v1/auth/renovar", servidor.takeRequest().path)
        val cierre = servidor.takeRequest()
        assertEquals("/api/v1/auth/cerrar-sesion", cierre.path)
        assertEquals("Bearer acceso-nuevo", cierre.getHeader("Authorization"))
        assertEquals("""{"tokenRenovacion":"renovacion-nueva"}""", cierre.body.readUtf8())
        assertTrue(terminado)
        assertNull(almacen.token)
    }

    @Test fun `si el servidor ya no acepta la renovacion no queda nada que revocar`() = runTest {
        // Arrange
        servidor.enqueue(MockResponse().setResponseCode(401))
        // Act
        val terminado = envio.ejecutar()
        // Assert
        assertTrue(terminado)
        assertEquals(1, servidor.requestCount)
        assertNull(almacen.token)
    }

    @Test fun `sin red conserva el pendiente para reintentar`() = runTest {
        // Arrange
        servidor.shutdown()
        // Act
        val terminado = envio.ejecutar()
        // Assert
        assertFalse(terminado)
        assertEquals("renovacion-vieja", almacen.token)
    }

    @Test fun `sin pendiente no llama al servidor`() = runTest {
        // Arrange
        almacen.token = null
        // Act
        val terminado = envio.ejecutar()
        // Assert
        assertTrue(terminado)
        assertEquals(0, servidor.requestCount)
    }
}
