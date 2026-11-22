package pe.killauru.app.sesion.datos

import kotlinx.coroutines.test.runTest
import kotlinx.serialization.json.Json
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.After
import org.junit.Before
import org.junit.Test
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import pe.killauru.app.sesion.dominio.ResultadoEliminacion
import pe.killauru.app.sesion.dominio.Rol
import pe.killauru.app.sesion.dominio.SolicitudRegistro
import pe.killauru.app.soporte.ValidadorContrato
import retrofit2.Retrofit
import retrofit2.converter.kotlinx.serialization.asConverterFactory
import java.time.Instant
import kotlin.test.assertEquals
import kotlin.test.assertIs

/**
 * FR-060, FR-061, FR-064, FR-067, FR-078: el cliente de cuenta sigue
 * `specs/003-modulo-identidad-accesos/contracts/identidad.openapi.yaml`. MockWebServer hace de API. Patrón AAA.
 */
class ContratoIdentidadTest {
    private val yaml = "003-modulo-identidad-accesos/contracts/identidad.openapi.yaml"
    private val json = Json { ignoreUnknownKeys = true }
    private lateinit var servidor: MockWebServer
    private lateinit var cuenta: ApiCuentaHttp

    private val sesionApp = """
        {"tokenAcceso":"jwt-acceso","expiraAccesoUtc":"2026-11-22T10:05:00Z","tokenRenovacion":"opaco-renovacion",
         "sesionVenceUtc":"2026-12-22T10:00:00Z","rol":"ConductorRegistrado"}
    """.trimIndent()

    private fun problema(estado: Int, codigo: String) =
        MockResponse().setResponseCode(estado)
            .setBody("""{"type":"t","title":"x","status":$estado,"codigo":"$codigo","correlacionId":"c"}""")

    @Before fun setUp() {
        servidor = MockWebServer().apply { start() }
        val retrofit = Retrofit.Builder().baseUrl(servidor.url("/"))
            .addConverterFactory(json.asConverterFactory("application/json".toMediaType()))
            .build()
        cuenta = ApiCuentaHttp(retrofit.create(IdentidadApi::class.java))
    }

    @After fun tearDown() = servidor.shutdown()

    @Test fun `la sesion de prueba cumple el esquema SesionApp del contrato`() {
        // Arrange
        val esquema = ValidadorContrato.esquema(yaml, "SesionApp")
        // Act
        val errores = ValidadorContrato.validarJson(sesionApp, esquema)
        // Assert
        assertEquals(emptyList(), errores)
    }

    @Test fun `registro envia RegistroSolicitud a la ruta del contrato y traduce la SesionApp`() = runTest {
        // Arrange
        servidor.enqueue(MockResponse().setResponseCode(201).setBody(sesionApp))
        // Act
        val resultado = cuenta.registrar(SolicitudRegistro("conductor@ejemplo.pe", "una-clave-larga", "1.2", "1.3"))
        // Assert
        val peticion = servidor.takeRequest()
        assertEquals("POST /api/v1/auth/registro", "${peticion.method} ${peticion.requestUrl?.encodedPath}")
        val cuerpo = ValidadorContrato.bodyDe(peticion)
        assertEquals(emptyList(), ValidadorContrato.validarJson(cuerpo, ValidadorContrato.esquema(yaml, "RegistroSolicitud")))
        assertIs<ResultadoCuenta.Exitoso>(resultado)
        assertEquals("opaco-renovacion", resultado.sesion.tokenRenovacion)
        assertEquals(Instant.parse("2026-12-22T10:00:00Z").toEpochMilli(), resultado.sesion.permiso.venceMs)
        assertEquals(Rol.ConductorRegistrado, resultado.sesion.rol)
    }

    @Test fun `registro con 409 correo_en_uso se traduce a CorreoEnUso`() = runTest {
        // Arrange
        servidor.enqueue(problema(409, "correo_en_uso"))
        // Act
        val resultado = cuenta.registrar(SolicitudRegistro("conductor@ejemplo.pe", "una-clave-larga", "1.2", "1.3"))
        // Assert
        assertEquals(ResultadoCuenta.CorreoEnUso, resultado)
    }

    @Test fun `inicio de sesion usa canal app y valida contra InicioSesionSolicitud`() = runTest {
        // Arrange
        servidor.enqueue(MockResponse().setBody(sesionApp))
        // Act
        val resultado = cuenta.iniciarSesion("conductor@ejemplo.pe", "una-clave-larga")
        // Assert
        val peticion = servidor.takeRequest()
        assertEquals("POST /api/v1/auth/inicio-sesion", "${peticion.method} ${peticion.requestUrl?.encodedPath}")
        val cuerpo = ValidadorContrato.bodyDe(peticion)
        assertEquals(emptyList(), ValidadorContrato.validarJson(cuerpo, ValidadorContrato.esquema(yaml, "InicioSesionSolicitud")))
        assertEquals("app", json.parseToJsonElement(cuerpo).jsonObject["canal"]!!.jsonPrimitive.content)
        assertIs<ResultadoCuenta.Exitoso>(resultado)
    }

    @Test fun `inicio de sesion con 401 da CredencialesInvalidas y con 429 LimiteExcedido`() = runTest {
        // Arrange
        servidor.enqueue(problema(401, "credenciales_invalidas"))
        servidor.enqueue(problema(429, "limite_excedido").addHeader("Retry-After", "60"))
        // Act
        val invalidas = cuenta.iniciarSesion("conductor@ejemplo.pe", "mal")
        val limite = cuenta.iniciarSesion("conductor@ejemplo.pe", "mal")
        // Assert
        assertEquals(ResultadoCuenta.CredencialesInvalidas, invalidas)
        assertEquals(ResultadoCuenta.LimiteExcedido, limite)
    }

    @Test fun `reenviar la verificacion usa la ruta de me y acepta el 202`() = runTest {
        // Arrange
        servidor.enqueue(MockResponse().setResponseCode(202))
        // Act
        val enviado = cuenta.reenviarVerificacion()
        // Assert
        val peticion = servidor.takeRequest()
        assertEquals("POST /api/v1/me/reenviar-verificacion", "${peticion.method} ${peticion.requestUrl?.encodedPath}")
        assertEquals(true, enviado)
    }

    @Test fun `eliminar la cuenta envia la contrasena a DELETE me y traduce 204 y 403`() = runTest {
        // Arrange
        servidor.enqueue(MockResponse().setResponseCode(204))
        servidor.enqueue(problema(403, "confirmacion_invalida"))
        // Act
        val eliminada = cuenta.eliminarCuenta("una-clave-larga")
        val rechazada = cuenta.eliminarCuenta("mal")
        // Assert
        val peticion = servidor.takeRequest()
        assertEquals("DELETE /api/v1/me", "${peticion.method} ${peticion.requestUrl?.encodedPath}")
        assertEquals("""{"contrasena":"una-clave-larga"}""", ValidadorContrato.bodyDe(peticion))
        assertEquals(ResultadoEliminacion.Eliminada, eliminada)
        assertEquals(ResultadoEliminacion.ConfirmacionInvalida, rechazada)
    }
}
