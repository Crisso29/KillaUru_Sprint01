package pe.killauru.app.sesion.datos

import kotlinx.coroutines.async
import kotlinx.coroutines.awaitAll
import kotlinx.coroutines.test.runTest
import kotlinx.serialization.json.Json
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.mockwebserver.Dispatcher
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import okhttp3.mockwebserver.RecordedRequest
import org.junit.After
import org.junit.Before
import org.junit.Test
import pe.killauru.app.comun.datos.red.ClienteHttp
import pe.killauru.app.seudonimo.datos.InterceptorSeudonimo
import pe.killauru.app.sesion.dominio.Rol
import pe.killauru.app.sesion.dominio.casos.AplicarSesionInvalidaUseCase
import pe.killauru.app.sesion.dominio.casos.LimpiezaLocalSesion
import pe.killauru.app.soporte.PreferenciasMemoria
import pe.killauru.app.soporte.SeudonimoMemoria
import pe.killauru.app.soporte.SesionMemoria
import pe.killauru.app.soporte.SesionesDePrueba
import retrofit2.Retrofit
import retrofit2.converter.kotlinx.serialization.asConverterFactory
import java.util.concurrent.atomic.AtomicInteger
import kotlin.test.assertEquals
import kotlin.test.assertNull

/**
 * FR-062, ADR-009, RN-21 c: la cadena real de interceptores (`ClienteHttp`, como en producción) añade el token de acceso,
 * no lo manda a las rutas de autenticación y renueva una sola vez ante 401. El seudónimo aislado: InterceptorSeudonimoTest.
 */
class RedAutenticadaTest {
    private lateinit var servidor: MockWebServer
    private lateinit var cliente: OkHttpClient
    private val sesiones = SesionMemoria()
    private val prefs = PreferenciasMemoria()
    private val seudonimo = SeudonimoMemoria("seudonimo-vigente")
    private val json = Json { ignoreUnknownKeys = true }
    private val renovaciones = AtomicInteger()
    @Volatile private var renovacionRechazada = false

    private val sesionRenovada = """
        {"tokenAcceso":"acceso-2","expiraAccesoUtc":"2026-11-22T10:05:00Z","tokenRenovacion":"renovacion-2",
         "sesionVenceUtc":"2026-12-22T10:00:00Z","rol":"ConductorRegistrado"}
    """.trimIndent()

    /** Responde 401 al token viejo y 200 al nuevo; cuenta las renovaciones. */
    private val api = object : Dispatcher() {
        override fun dispatch(request: RecordedRequest): MockResponse = when {
            request.path == "/api/v1/auth/renovar" && renovacionRechazada -> MockResponse().setResponseCode(401)
            request.path == "/api/v1/auth/renovar" -> MockResponse().setBody(sesionRenovada).also { renovaciones.incrementAndGet() }
            request.getHeader("Authorization") == "Bearer acceso-1" -> MockResponse().setResponseCode(401)
            else -> MockResponse().setResponseCode(200)
        }
    }

    @Before fun setUp() {
        servidor = MockWebServer().apply { dispatcher = api; start() }
        var identidad: IdentidadApi? = null
        val invalida = AplicarSesionInvalidaUseCase(sesiones, LimpiezaLocalSesion(sesiones, prefs))
        cliente = ClienteHttp.construir(
            InterceptorToken(sesiones),
            InterceptorSeudonimo(seudonimo),
            RenovadorToken(sesiones, { identidad!! }, invalida),
        )
        identidad = Retrofit.Builder().baseUrl(servidor.url("/")).client(cliente)
            .addConverterFactory(json.asConverterFactory("application/json".toMediaType()))
            .build().create(IdentidadApi::class.java)
    }

    @After fun tearDown() = servidor.shutdown()

    private fun pedir(ruta: String, configurar: Request.Builder.() -> Unit = {}): Int =
        cliente.newCall(Request.Builder().url(servidor.url(ruta)).apply(configurar).build()).execute().use { it.code }

    @Test fun `con sesion cada peticion lleva el token de acceso y no el seudonimo`() {
        // Arrange
        sesiones.actual = SesionesDePrueba.de(Rol.ConductorRegistrado, tokenAcceso = "acceso-2")
        // Act
        pedir("/api/v1/anomalies")
        // Assert
        val peticion = servidor.takeRequest()
        assertEquals("Bearer acceso-2", peticion.getHeader("Authorization"))
        assertNull(peticion.getHeader("X-KillaUru-Seudonimo"))
    }

    /** La API responde 401 a un token vencido incluso en rutas públicas: renovar no debe llevarlo. */
    @Test fun `las rutas de autenticacion no llevan el token de acceso`() {
        // Arrange
        sesiones.actual = SesionesDePrueba.de(Rol.ConductorRegistrado, tokenAcceso = "acceso-vencido")
        // Act
        pedir("/api/v1/auth/inicio-sesion")
        // Assert
        assertNull(servidor.takeRequest().getHeader("Authorization"))
    }

    @Test fun `ante 401 renueva con el token de renovacion y reintenta con el nuevo`() {
        // Arrange
        sesiones.actual = SesionesDePrueba.de(Rol.ConductorRegistrado, tokenAcceso = "acceso-1")
        // Act
        val codigo = pedir("/api/v1/anomalies")
        // Assert
        assertEquals(200, codigo)
        assertEquals("acceso-2", sesiones.actual?.tokenAcceso)
        assertEquals("renovacion-2", sesiones.actual?.tokenRenovacion)
    }

    /** FR-062 y ADR-009: reusar el mismo token de renovación pasados 60 s revoca la sesión; hay que renovar una vez. */
    @Test fun `varios 401 a la vez producen una sola renovacion`() = runTest {
        // Arrange
        sesiones.actual = SesionesDePrueba.de(Rol.ConductorRegistrado, tokenAcceso = "acceso-1")
        // Act
        val codigos = List(4) { async(kotlinx.coroutines.Dispatchers.IO) { pedir("/api/v1/anomalies") } }.awaitAll()
        // Assert
        assertEquals(List(4) { 200 }, codigos)
        assertEquals(1, renovaciones.get())
    }

    /** RN-21 c: si el servidor rechaza la renovación (sesión revocada o cuenta desactivada), se borra la sesión local. */
    @Test fun `si la renovacion se rechaza se invalida la sesion local`() {
        // Arrange
        sesiones.actual = SesionesDePrueba.de(Rol.ConductorRegistrado, tokenAcceso = "acceso-1")
        renovacionRechazada = true
        // Act
        val codigo = pedir("/api/v1/anomalies")
        // Assert
        assertEquals(401, codigo)
        assertNull(sesiones.actual)
        assertEquals(1, prefs.limpiezas)
    }
}
