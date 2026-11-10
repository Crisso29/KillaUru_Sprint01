package pe.killauru.app.actualizador.datos

import android.content.Context
import androidx.test.core.app.ApplicationProvider
import com.jakewharton.retrofit2.converter.kotlinx.serialization.asConverterFactory
import kotlinx.coroutines.test.runTest
import kotlinx.serialization.json.Json
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.After
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import pe.killauru.app.mapa.datos.AlmacenArchivos
import pe.killauru.app.mapa.datos.CdnApi
import pe.killauru.app.mapa.datos.DescargaReanudable
import pe.killauru.app.mapa.datos.VerificadorSha256
import retrofit2.Retrofit
import java.security.MessageDigest
import kotlin.test.assertIs
import kotlin.test.assertFalse
import kotlin.test.assertEquals

/**
 * T086 / FR-041, SC-011: descarga reanudable via HTTP Range.
 * MockWebServer simula el CDN. Patrón AAA en cada test.
 */
@RunWith(RobolectricTestRunner::class)
class DescargaReanudableTest {

    private lateinit var server: MockWebServer
    private lateinit var descarga: DescargaReanudable
    private lateinit var almacen: AlmacenArchivos

    private val contenidoCompleto = "0123456789abcdef".repeat(100).toByteArray()
    private val sha256Correcto: String by lazy { sha256De(contenidoCompleto) }

    @Before fun setUp() {
        val ctx = ApplicationProvider.getApplicationContext<Context>()
        almacen = AlmacenArchivos(ctx)
        server = MockWebServer()
        server.start()
        val retrofit = Retrofit.Builder()
            .baseUrl(server.url("/"))
            .client(OkHttpClient())
            .addConverterFactory(Json.asConverterFactory("application/json".toMediaType()))
            .build()
        descarga = DescargaReanudable(retrofit.create(CdnApi::class.java), almacen, VerificadorSha256())
    }

    @After fun tearDown() {
        server.shutdown()
        // Limpieza del almacén para no contaminar otros tests
        almacen.listarMapa().forEach { it.delete() }
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private fun sha256De(bytes: ByteArray): String {
        val digest = MessageDigest.getInstance("SHA-256")
        return digest.digest(bytes).joinToString("") { "%02x".format(it) }
    }

    // ── tests ────────────────────────────────────────────────────────────────

    @Test fun `descarga completa exitosa con SHA-256 correcto`() = runTest {
        // Arrange
        server.enqueue(
            MockResponse()
                .setResponseCode(200)
                .setBody(okio.Buffer().write(contenidoCompleto))
        )
        // Act
        val resultado = descarga.descargar(server.url("/tile.pmtiles").toString(), "tile.pmtiles", sha256Correcto)
        // Assert
        assertIs<DescargaReanudable.Resultado.Exitosa>(resultado)
    }

    @Test fun `reanuda con encabezado Range cuando hay parcial previo`() = runTest {
        // Arrange - 62 % ya descargado
        val descargado = (contenidoCompleto.size * 0.62).toLong()
        val parcial = almacen.archivoParcial("tile.pmtiles")
        parcial.writeBytes(contenidoCompleto.take(descargado.toInt()).toByteArray())

        val resto = contenidoCompleto.drop(descargado.toInt()).toByteArray()
        server.enqueue(
            MockResponse()
                .setResponseCode(206)
                .addHeader("Content-Range", "bytes $descargado-${contenidoCompleto.size - 1}/${contenidoCompleto.size}")
                .setBody(okio.Buffer().write(resto))
        )
        // Act
        val resultado = descarga.descargar(server.url("/tile.pmtiles").toString(), "tile.pmtiles", sha256Correcto)
        // Assert
        val peticion = server.takeRequest()
        assertEquals("bytes=$descargado-", peticion.getHeader("Range"))
        assertIs<DescargaReanudable.Resultado.Exitosa>(resultado)
    }

    @Test fun `416 descarta el parcial y descarga desde cero`() = runTest {
        // Arrange - hay un parcial corrupto
        val parcial = almacen.archivoParcial("tile.pmtiles")
        parcial.writeBytes(byteArrayOf(0x00, 0x01))  // parcial inválido

        // Primera respuesta: 416; segunda: descarga completa
        server.enqueue(MockResponse().setResponseCode(416))
        server.enqueue(
            MockResponse()
                .setResponseCode(200)
                .setBody(okio.Buffer().write(contenidoCompleto))
        )
        // Act
        val resultado = descarga.descargar(server.url("/tile.pmtiles").toString(), "tile.pmtiles", sha256Correcto)
        // Assert - segunda petición no tiene Range (descarga completa desde 0)
        server.takeRequest()  // primera (con Range → 416)
        val segunda = server.takeRequest()
        assertFalse(segunda.headers.names().contains("Range"), "tras 416 no debe enviar Range")
        assertIs<DescargaReanudable.Resultado.Exitosa>(resultado)
    }

    @Test fun `SHA-256 incorrecto borra el archivo y retorna Fallida`() = runTest {
        // Arrange
        server.enqueue(
            MockResponse()
                .setResponseCode(200)
                .setBody(okio.Buffer().write(contenidoCompleto))
        )
        val sha256Malo = "0".repeat(64)
        // Act
        val resultado = descarga.descargar(server.url("/tile.pmtiles").toString(), "tile.pmtiles", sha256Malo)
        // Assert
        assertIs<DescargaReanudable.Resultado.Fallida>(resultado)
        assertFalse(almacen.archivoMapa("tile.pmtiles").exists(), "el archivo con hash incorrecto debe borrarse")
    }
}
