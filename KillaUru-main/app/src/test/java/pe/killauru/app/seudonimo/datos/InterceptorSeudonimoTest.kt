package pe.killauru.app.seudonimo.datos

import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.After
import org.junit.Before
import org.junit.Test
import pe.killauru.app.soporte.SeudonimoMemoria
import kotlin.test.assertEquals
import kotlin.test.assertNull

/** T035 / FR-040, RF-27, D2: `X-KillaUru-Seudonimo` solo en peticiones anónimas y sin pisar el del lote. Patrón AAA. */
class InterceptorSeudonimoTest {
    private val server = MockWebServer()
    private val client = OkHttpClient.Builder()
        .addInterceptor(InterceptorSeudonimo(SeudonimoMemoria("seudonimo-vigente")))
        .build()

    @Before fun setUp() = server.start()

    @After fun tearDown() = server.shutdown()

    private fun enviar(configurar: Request.Builder.() -> Unit = {}) {
        server.enqueue(MockResponse().setResponseCode(200))
        client.newCall(Request.Builder().url(server.url("/test")).apply(configurar).build()).execute().close()
    }

    @Test fun `sin token lleva el seudonimo vigente`() {
        // Arrange — petición anónima
        // Act
        enviar()
        // Assert
        assertEquals("seudonimo-vigente", server.takeRequest().getHeader("X-KillaUru-Seudonimo"))
    }

    @Test fun `con Authorization no lleva seudonimo`() {
        // Arrange
        val conToken: Request.Builder.() -> Unit = { header("Authorization", "Bearer token123") }
        // Act
        enviar(conToken)
        // Assert
        assertNull(server.takeRequest().getHeader("X-KillaUru-Seudonimo"))
    }

    /** D2: el lote lleva el seudónimo con el que se detectó, aunque luego haya rotado. */
    @Test fun `no reemplaza el seudonimo que ya trae la peticion`() {
        // Arrange
        val delLote: Request.Builder.() -> Unit = { header("X-KillaUru-Seudonimo", "seudonimo-del-lote") }
        // Act
        enviar(delLote)
        // Assert
        assertEquals("seudonimo-del-lote", server.takeRequest().getHeader("X-KillaUru-Seudonimo"))
    }
}
