package pe.killauru.app.comun.datos.red

import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.After
import org.junit.Before
import org.junit.Test
import kotlin.test.assertEquals
import kotlin.test.assertNull

/** T035 / FR-040: X-KillaUru-Seudonimo solo en peticiones sin Authorization. */
class InterceptorSeudonimoTest {

    private val server = MockWebServer()
    private val seudonimo = "test-seudonimo-uuid"

    private val client = OkHttpClient.Builder()
        .addInterceptor(InterceptorSeudonimo { seudonimo })
        .build()

    @Before
    fun setUp() { server.start() }

    @After
    fun tearDown() { server.shutdown() }

    @Test
    fun `sin token lleva X-KillaUru-Seudonimo`() {
        // Arrange
        server.enqueue(MockResponse().setResponseCode(200))
        val request = Request.Builder().url(server.url("/test")).build()
        // Act
        client.newCall(request).execute().close()
        // Assert
        val recibida = server.takeRequest()
        assertEquals(seudonimo, recibida.getHeader("X-KillaUru-Seudonimo"))
    }

    @Test
    fun `con Authorization no lleva X-KillaUru-Seudonimo`() {
        // Arrange
        server.enqueue(MockResponse().setResponseCode(200))
        val request = Request.Builder()
            .url(server.url("/test"))
            .header("Authorization", "Bearer token123")
            .build()
        // Act
        client.newCall(request).execute().close()
        // Assert
        val recibida = server.takeRequest()
        assertNull(recibida.getHeader("X-KillaUru-Seudonimo"))
    }
}
